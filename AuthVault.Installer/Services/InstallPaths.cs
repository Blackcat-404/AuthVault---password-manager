namespace AuthVault.Installer.Services;

public static class InstallPaths
{
    public static readonly string InstallDir  = Path.Combine(GetRealHome(), ".authvault");
    public static readonly string EnvFile     = Path.Combine(InstallDir, ".env");
    public static readonly string ComposeFile = Path.Combine(InstallDir, "docker-compose.yml");
    public static readonly string CertsDir    = Path.Combine(InstallDir, "certs");
    public static readonly string CaPath      = Path.Combine(CertsDir, "authvault-ca.crt");
    public static readonly string PfxPath     = Path.Combine(CertsDir, "localhost.pfx");

    // When running with sudo, UserProfile returns /root instead of the real user's home.
    // SUDO_USER contains the original username.
    internal static string GetRealHome()
    {
        var sudoUser = Environment.GetEnvironmentVariable("SUDO_USER");
        if (!string.IsNullOrEmpty(sudoUser))
            return Path.Combine("/home", sudoUser);
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
}
