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
            "uninstall" => await UninstallCommand(platform, docker, certs),
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
        await platform.ChownToRealUserAsync(InstallPaths.InstallDir);

        // 5. Generate HTTPS certificates
        if (!await certs.SetupAsync(cfg.HttpsPort)) return 1;

        // 6. Write .env and docker-compose.yml
        config.WriteEnvFile(cfg);
        config.WriteComposeFile(cfg);

        // 7. Pull images and start containers (migrations run at app startup)
        if (!await docker.ComposeUpAsync())
        {
            await docker.DiagnoseFailureAsync();
            return 1;
        }

        Display.Success("\nAuthVault installed and running!");
        Display.Info($"Open [bold]https://localhost:{cfg.HttpsPort}[/] in your browser.");

        // 8. Offer to install binary to PATH (Linux/macOS only)
        if (platform.CurrentOS != OS.Windows)
            InstallToPath(platform);

        return 0;
    }

    static async Task<int> UpdateCommand(DockerService docker)
    {
        Display.Section("Updating AuthVault");

        bool ok = await docker.ComposeUpAsync();

        if (ok) Display.Success("AuthVault updated.");
        return ok ? 0 : 1;
    }

    static async Task<int> UninstallCommand(PlatformService platform, DockerService docker, CertificateService certs)
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
        await certs.DeleteCertFilesAsync();

        // Remove from /usr/local/bin if installed there
        if (platform.CurrentOS != OS.Windows)
            RemoveFromPath(platform);

        Display.Success("AuthVault uninstalled.");
        Display.Info("Database volume [bold]authvault-data[/] was kept. To also delete vault data:");
        Display.Info("  [bold]docker volume rm authvault-data[/]");
        return 0;
    }

    static void InstallToPath(PlatformService platform)
    {
        const string target = "/usr/local/bin/authvault";

        Display.Section("Add to PATH");
        Display.Info("Moving the installer to [bold]/usr/local/bin/authvault[/] lets you run");
        Display.Info("[bold]authvault start/stop/update[/] from any directory.");
        Display.Info("[yellow]If you skip this, you must always run the binary with[/] [bold]./authvault-...[/]");
        Display.Info("from the folder where it was downloaded.\n");

        if (!Display.Confirm("Install authvault to /usr/local/bin/?"))
        {
            Display.Info("Skipped. Run commands with [bold]./authvault-linux-x64 <command>[/] (or your platform binary).");
            return;
        }

        var self = Environment.ProcessPath!;

        try
        {
            if (File.Exists(target))
                File.Delete(target);

            File.Copy(self, target);

            // chmod +x
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "chmod", Arguments = $"+x {target}",
                UseShellExecute = false
            })?.WaitForExit();

            Display.Success($"Installed to {target}. You can now run [bold]authvault <command>[/] from anywhere.");
        }
        catch
        {
            Display.Warning("Could not write to /usr/local/bin/ — try running the installer with sudo.");
        }
    }

    static void RemoveFromPath(PlatformService platform)
    {
        const string target = "/usr/local/bin/authvault";

        if (!File.Exists(target)) return;

        try
        {
            File.Delete(target);
            Display.Success("Removed authvault from /usr/local/bin/");
        }
        catch
        {
            Display.Warning($"Could not remove {target} — delete it manually with: [bold]sudo rm {target}[/]");
        }
    }

    static int ShowHelp()
    {
        Display.Help();
        return 0;
    }
}
