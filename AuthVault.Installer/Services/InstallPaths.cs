namespace AuthVault.Installer.Services;

public static class InstallPaths
{
    public static readonly string InstallDir   = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".authvault");

    public static readonly string EnvFile      = Path.Combine(InstallDir, ".env");
    public static readonly string ComposeFile  = Path.Combine(InstallDir, "docker-compose.yml");
    public static readonly string CertsDir     = Path.Combine(InstallDir, "certs");
    public static readonly string CaPath       = Path.Combine(CertsDir, "authvault-ca.crt");
    public static readonly string PfxPath      = Path.Combine(CertsDir, "localhost.pfx");
}
