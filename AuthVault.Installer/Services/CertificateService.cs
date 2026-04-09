using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AuthVault.Installer.UI;

namespace AuthVault.Installer.Services;

public class CertificateService(PlatformService platform)
{
    const string CaName = "AuthVault Local CA";

    // Public API

    public async Task<bool> SetupAsync(int httpsPort)
    {
        Display.Section("Setting up HTTPS certificates");

        Directory.CreateDirectory(InstallPaths.CertsDir);

        bool ok = await Display.SpinnerAsync("Generating certificates...", async () =>
        {
            await Task.Yield();
            return GenerateCerts();
        });

        if (!ok) return false;
        Display.Success("Certificates generated");

        await TrustCaAsync();
        return true;
    }

    public async Task RemoveTrustAsync()
    {
        Display.Step("Removing certificate trust...");
        await RemoveCaFromTrustStoresAsync();
        Display.Success("Certificate trust removed");
    }

    public void DeleteCertFiles()
    {
        if (Directory.Exists(InstallPaths.CertsDir))
        {
            Directory.Delete(InstallPaths.CertsDir, recursive: true);
            Display.Success("Certificate files deleted");
        }
    }

    // Certificate generation

    bool GenerateCerts()
    {
        try
        {
            // --- CA ---
            using var caKey = RSA.Create(4096);
            var caReq = new CertificateRequest(
                new X500DistinguishedName($"CN={CaName}, O=AuthVault"),
                caKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            caReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, critical: true));
            caReq.CertificateExtensions.Add(new X509KeyUsageExtension(
                X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, critical: true));
            caReq.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(caReq.PublicKey, critical: false));

            using var caCert = caReq.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddYears(10));

            File.WriteAllText(InstallPaths.CaPath, ExportCertPem(caCert));

            // --- Server cert ---
            using var serverKey = RSA.Create(2048);
            var serverReq = new CertificateRequest(
                new X500DistinguishedName("CN=localhost"),
                serverKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            var san = new SubjectAlternativeNameBuilder();
            san.AddDnsName("localhost");
            san.AddIpAddress(IPAddress.Loopback);
            san.AddIpAddress(IPAddress.IPv6Loopback);
            serverReq.CertificateExtensions.Add(san.Build());
            serverReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, critical: false));
            serverReq.CertificateExtensions.Add(new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, critical: false));
            serverReq.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
                new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, critical: false));

            var serial = new byte[16];
            RandomNumberGenerator.Fill(serial);

            using var serverCert = serverReq.Create(
                caCert,
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddYears(1),
                serial);

            using var serverCertWithKey = serverCert.CopyWithPrivateKey(serverKey);
            File.WriteAllBytes(InstallPaths.PfxPath, serverCertWithKey.Export(X509ContentType.Pfx));

            return true;
        }
        catch (Exception ex)
        {
            Display.Error($"Certificate generation failed: {ex.Message}");
            return false;
        }
    }

    // Trust store management

    async Task TrustCaAsync()
    {
        Display.Step("Adding CA to system trust store...");

        switch (platform.CurrentOS)
        {
            case OS.Windows: TrustWindows();          break;
            case OS.MacOS:   await TrustMacOSAsync(); break;
            case OS.Linux:   await TrustLinuxAsync(); break;
        }

        if (platform.CurrentOS != OS.Windows)
            await TrustFirefoxNssAsync();
    }

    void TrustWindows()
    {
        try
        {
            var cert = new X509Certificate2(InstallPaths.CaPath);
            using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close();
            Display.Success("CA trusted in Windows certificate store");
        }
        catch
        {
            Display.Warning("Could not add CA to Windows store. Try running as Administrator.");
        }
    }

    async Task TrustMacOSAsync()
    {
        var (code, _, _) = await platform.RunAsync(
            "security",
            $"add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain \"{InstallPaths.CaPath}\"",
            sudo: true);

        if (code == 0) Display.Success("CA trusted in macOS Keychain");
        else           Display.Warning("Could not add CA to macOS Keychain. Run with sudo or add manually.");
    }

    async Task TrustLinuxAsync()
    {
        switch (platform.Distro)
        {
            case LinuxDistro.Arch:
                await platform.RunAsync("cp", $"\"{InstallPaths.CaPath}\" /etc/ca-certificates/trust-source/anchors/authvault-ca.crt", sudo: true);
                await platform.RunAsync("trust", "extract-compat", sudo: true);
                break;
            case LinuxDistro.Debian:
                await platform.RunAsync("cp", $"\"{InstallPaths.CaPath}\" /usr/local/share/ca-certificates/authvault-ca.crt", sudo: true);
                await platform.RunAsync("update-ca-certificates", "", sudo: true);
                break;
            case LinuxDistro.Fedora:
                await platform.RunAsync("cp", $"\"{InstallPaths.CaPath}\" /etc/pki/ca-trust/source/anchors/authvault-ca.crt", sudo: true);
                await platform.RunAsync("update-ca-trust", "extract", sudo: true);
                break;
            default:
                Display.Warning("Unknown distro — skipping system CA trust. Add manually if needed.");
                return;
        }
        Display.Success("CA trusted in system certificate store");
    }

    async Task TrustFirefoxNssAsync()
    {
        var (avail, _, _) = await platform.RunAsync("which", "certutil");
        if (avail != 0)
        {
            Display.Warning("certutil not found — Firefox/LibreWolf may not trust the cert automatically.");
            return;
        }

        int trusted = 0;
        foreach (var dir in GetBrowserProfileDirs())
        {
            var (code, _, _) = await platform.RunAsync(
                "certutil",
                $"-d sql:\"{dir}\" -A -t \"CT,,\" -n \"{CaName}\" -i \"{InstallPaths.CaPath}\"");
            if (code == 0) trusted++;
        }

        if (trusted > 0) Display.Success($"CA trusted in {trusted} Firefox/LibreWolf profile(s)");
        else             Display.Warning("No Firefox/LibreWolf profiles found.");
    }

    // Trust removal

    async Task RemoveCaFromTrustStoresAsync()
    {
        switch (platform.CurrentOS)
        {
            case OS.Windows: RemoveTrustWindows(); break;
            case OS.MacOS:
                await platform.RunAsync("security",
                    $"delete-certificate -c \"{CaName}\" /Library/Keychains/System.keychain", sudo: true);
                break;
            case OS.Linux:
                await RemoveTrustLinuxAsync();
                break;
        }

        if (platform.CurrentOS != OS.Windows)
            await RemoveFirefoxNssTrustAsync();
    }

    void RemoveTrustWindows()
    {
        try
        {
            using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            var found = store.Certificates.Find(X509FindType.FindBySubjectName, CaName, false);
            foreach (var cert in found) store.Remove(cert);
            store.Close();
        }
        catch { /* best-effort */ }
    }

    async Task RemoveTrustLinuxAsync()
    {
        switch (platform.Distro)
        {
            case LinuxDistro.Arch:
                await platform.RunAsync("rm", "-f /etc/ca-certificates/trust-source/anchors/authvault-ca.crt", sudo: true);
                await platform.RunAsync("trust", "extract-compat", sudo: true);
                break;
            case LinuxDistro.Debian:
                await platform.RunAsync("rm", "-f /usr/local/share/ca-certificates/authvault-ca.crt", sudo: true);
                await platform.RunAsync("update-ca-certificates", "--fresh", sudo: true);
                break;
            case LinuxDistro.Fedora:
                await platform.RunAsync("rm", "-f /etc/pki/ca-trust/source/anchors/authvault-ca.crt", sudo: true);
                await platform.RunAsync("update-ca-trust", "extract", sudo: true);
                break;
        }
    }

    async Task RemoveFirefoxNssTrustAsync()
    {
        var (avail, _, _) = await platform.RunAsync("which", "certutil");
        if (avail != 0) return;

        foreach (var dir in GetBrowserProfileDirs())
            await platform.RunAsync("certutil", $"-d sql:\"{dir}\" -D -n \"{CaName}\"");
    }

    // Helpers

    static IEnumerable<string> GetBrowserProfileDirs()
    {
        var home = InstallPaths.GetRealHome();

        var roots = new[]
        {
            Path.Combine(home, ".mozilla",  "firefox"),
            Path.Combine(home, ".librewolf"),
            Path.Combine(home, "Library", "Application Support", "Firefox",   "Profiles"),
            Path.Combine(home, "Library", "Application Support", "LibreWolf", "Profiles"),
        };

        foreach (var root in roots)
        {
            if (!Directory.Exists(root)) continue;
            foreach (var dir in Directory.GetDirectories(root))
                if (File.Exists(Path.Combine(dir, "cert9.db")))
                    yield return dir;
        }
    }

    static string ExportCertPem(X509Certificate2 cert)
    {
        var sb = new StringBuilder();
        sb.AppendLine("-----BEGIN CERTIFICATE-----");
        sb.AppendLine(Convert.ToBase64String(cert.RawData, Base64FormattingOptions.InsertLineBreaks));
        sb.AppendLine("-----END CERTIFICATE-----");
        return sb.ToString();
    }
}
