using AuthVault.Installer.Services;
using AuthVault.Installer.UI;

namespace AuthVault.Installer;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Display.Banner();

        var command = args.Length > 0 ? args[0].ToLower() : "help";

        var platform = new PlatformService();
        var certs    = new CertificateService(platform);
        var docker   = new DockerService(platform);
        var config   = new ConfigurationService();
        var app      = new ApplicationService(docker);

        return command switch
        {
            "install"   => await InstallCommand(platform, docker, certs, config),
            "start"     => await app.StartAsync(),
            "stop"      => await app.StopAsync(),
            "status"    => await app.StatusAsync(),
            "update"    => await UpdateCommand(docker),
            "uninstall" => await UninstallCommand(docker, certs),
            _           => ShowHelp()
        };
    }

    static async Task<int> InstallCommand(
        PlatformService platform,
        DockerService docker,
        CertificateService certs,
        ConfigurationService config)
    {
        Display.Section("Starting installation");

        // 1. Ensure Docker + Compose are available
        if (!await docker.EnsureDockerAsync()) return 1;

        // 2. Install nss-tools for Firefox cert trust (Linux only)
        await platform.EnsureNssToolsAsync();

        // 3. Collect configuration from user
        var cfg = config.PromptConfiguration();

        // 4. Create install directory (~/.authvault/)
        Directory.CreateDirectory(InstallPaths.InstallDir);

        // 5. Generate HTTPS certificates
        if (!await certs.SetupAsync(cfg.HttpsPort)) return 1;

        // 6. Write .env and docker-compose.yml
        config.WriteEnvFile(cfg);
        config.WriteComposeFile(cfg);

        // 7. Pull images and start containers (migrations run at app startup)
        if (!await docker.ComposeUpAsync()) return 1;

        Display.Success("\nAuthVault installed and running!");
        Display.Info($"Open [bold]https://localhost:{cfg.HttpsPort}[/] in your browser.");
        return 0;
    }

    static async Task<int> UpdateCommand(DockerService docker)
    {
        Display.Section("Updating AuthVault");

        bool ok = await docker.ComposeUpAsync();

        if (ok) Display.Success("AuthVault updated.");
        return ok ? 0 : 1;
    }

    static async Task<int> UninstallCommand(DockerService docker, CertificateService certs)
    {
        Display.Section("Uninstalling AuthVault");

        if (!Display.Confirm("This will stop and remove all AuthVault containers and trusted certificates. Continue?"))
        {
            Display.Info("Uninstall cancelled.");
            return 0;
        }

        Display.Step("Stopping and removing containers...");
        await docker.ComposeDownAsync(withVolumes: false);

        await certs.RemoveTrustAsync();
        certs.DeleteCertFiles();

        Display.Success("AuthVault uninstalled.");
        Display.Info("Database volume [bold]authvault-data[/] was kept. To also delete vault data:");
        Display.Info("  [bold]docker volume rm authvault-data[/]");
        return 0;
    }

    static int ShowHelp()
    {
        Display.Help();
        return 0;
    }
}
