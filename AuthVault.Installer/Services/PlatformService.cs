using System.Diagnostics;
using System.Runtime.InteropServices;
using AuthVault.Installer.UI;

namespace AuthVault.Installer.Services;

public enum OS { Windows, Linux, MacOS }
public enum LinuxDistro { Arch, Debian, Fedora, OpenSuse, Unknown }

public class PlatformService
{
    public OS CurrentOS { get; } = DetectOS();
    public LinuxDistro Distro { get; } = DetectDistro();

    // OS detection

    static OS DetectOS()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return OS.Windows;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))     return OS.MacOS;
        return OS.Linux;
    }

    static LinuxDistro DetectDistro()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return LinuxDistro.Unknown;
        if (!File.Exists("/etc/os-release")) return LinuxDistro.Unknown;

        var content = File.ReadAllText("/etc/os-release").ToLowerInvariant();

        if (content.Contains("arch") || content.Contains("artix") || content.Contains("manjaro"))
            return LinuxDistro.Arch;
        if (content.Contains("ubuntu") || content.Contains("debian") || content.Contains("mint") || content.Contains("pop"))
            return LinuxDistro.Debian;
        if (content.Contains("fedora") || content.Contains("rhel") || content.Contains("centos") || content.Contains("rocky"))
            return LinuxDistro.Fedora;
        if (content.Contains("opensuse") || content.Contains("suse"))
            return LinuxDistro.OpenSuse;

        return LinuxDistro.Unknown;
    }

    // Shell execution

    public async Task<(int ExitCode, string Output, string Error)> RunAsync(
        string command, string arguments, string? workDir = null, bool sudo = false)
    {
        string cmd;
        string args;

        if (CurrentOS == OS.Windows)
        {
            cmd  = "cmd.exe";
            args = $"/c {command} {arguments}";
        }
        else if (sudo)
        {
            cmd  = "sudo";
            args = $"{command} {arguments}";
        }
        else
        {
            cmd  = command;
            args = arguments;
        }

        var psi = new ProcessStartInfo(cmd, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            WorkingDirectory       = workDir ?? Directory.GetCurrentDirectory()
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException($"Failed to start: {cmd}");

        var output = await process.StandardOutput.ReadToEndAsync();
        var error  = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return (process.ExitCode, output.Trim(), error.Trim());
    }

    public async Task<bool> IsCommandAvailableAsync(string command)
    {
        var check = CurrentOS == OS.Windows ? "where" : "which";
        var (code, _, _) = await RunAsync(check, command);
        return code == 0;
    }

    // Prerequisites

    public async Task<bool> CheckDotnetAsync()
    {
        var (code, output, _) = await RunAsync("dotnet", "--version");
        if (code != 0)
        {
            Display.Error(".NET SDK not found. Please install .NET 8 from https://dot.net");
            return false;
        }
        Display.Success($".NET SDK {output}");
        return true;
    }

    public async Task EnsureNssToolsAsync()
    {
        if (CurrentOS != OS.Linux) return;
        if (await IsCommandAvailableAsync("certutil")) return;

        Display.Step("Installing nss-tools for Firefox certificate trust...");

        var (code, _, err) = Distro switch
        {
            LinuxDistro.Arch    => await RunAsync("pacman", "-S --noconfirm nss", sudo: true),
            LinuxDistro.Debian  => await RunAsync("apt-get", "install -y libnss3-tools", sudo: true),
            LinuxDistro.Fedora  => await RunAsync("dnf", "install -y nss-tools", sudo: true),
            LinuxDistro.OpenSuse=> await RunAsync("zypper", "install -y mozilla-nss-tools", sudo: true),
            _                   => (1, "", "Unknown distro")
        };

        if (code == 0)
            Display.Success("nss-tools installed");
        else
            Display.Warning("Could not install nss-tools. Firefox may not trust the certificate automatically.");
    }

    // Ownership

    public async Task ChownToRealUserAsync(string path)
    {
        if (CurrentOS == OS.Windows) return;

        var sudoUser = Environment.GetEnvironmentVariable("SUDO_USER");
        if (string.IsNullOrEmpty(sudoUser)) return; // not running under sudo, already correct owner

        await RunAsync("chown", $"-R {sudoUser}:{sudoUser} \"{path}\"", sudo: true);
    }

    // Package manager helpers

    public async Task<(int ExitCode, string Output, string Error)> InstallPackageAsync(string package)
    {
        return CurrentOS switch
        {
            OS.Windows => await RunAsync("winget", $"install -e --id {package} --silent"),
            OS.MacOS   => await RunAsync("brew",   $"install {package}"),
            OS.Linux   => Distro switch
            {
                LinuxDistro.Arch    => await RunAsync("pacman", $"-S --noconfirm {package}", sudo: true),
                LinuxDistro.Debian  => await RunAsync("apt-get",$"install -y {package}",     sudo: true),
                LinuxDistro.Fedora  => await RunAsync("dnf",    $"install -y {package}",     sudo: true),
                LinuxDistro.OpenSuse=> await RunAsync("zypper", $"install -y {package}",     sudo: true),
                _                   => (1, "", "Unknown distro")
            },
            _ => (1, "", "Unsupported OS")
        };
    }
}
