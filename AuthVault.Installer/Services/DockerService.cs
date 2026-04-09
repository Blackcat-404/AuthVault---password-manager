using AuthVault.Installer.UI;

namespace AuthVault.Installer.Services;

public class DockerService(PlatformService platform)
{
    // Docker / Compose availability

    public async Task<bool> EnsureDockerAsync()
    {
        Display.Section("Checking Docker");

        if (!await IsDockerAvailableAsync())
        {
            Display.Warning("Docker not found.");

            if (platform.CurrentOS == OS.Linux)
            {
                if (!await InstallDockerLinuxAsync()) return false;
            }
            else
            {
                Display.Error("Docker is required but not installed.");
                Display.Info("Download and install Docker Desktop:");
                Display.Link("https://www.docker.com/products/docker-desktop");
                Display.Info("Then run [bold]authvault install[/] again.");
                return false;
            }
        }

        var (_, ver, _) = await platform.RunAsync("docker", "--version");
        Display.Success($"Docker found — {ver.Trim()}");

        if (!await IsComposeAvailableAsync())
        {
            Display.Error("Docker Compose not found. Install Docker Desktop or the Compose plugin.");
            Display.Link("https://docs.docker.com/compose/install/");
            return false;
        }

        Display.Success("Docker Compose available");
        return true;
    }

    // Compose operations

    public async Task<bool> ComposeUpAsync()
    {
        Display.Section("Starting AuthVault");

        bool ok = await Display.SpinnerAsync(
            "Pulling images and starting containers (this may take a while on first run)...",
            async () =>
            {
                var (code, _, err) = await platform.RunAsync(
                    "docker", "compose up --pull always -d",
                    workDir: InstallPaths.InstallDir);

                if (code != 0)
                {
                    Display.Error($"docker compose up failed:\n{err}");
                    return false;
                }
                return true;
            });

        return ok;
    }

    public async Task<bool> ComposeDownAsync(bool withVolumes = false)
    {
        var args = withVolumes ? "compose down --volumes" : "compose down";
        var (code, _, err) = await platform.RunAsync(
            "docker", args, workDir: InstallPaths.InstallDir);

        if (code != 0)
        {
            Display.Error($"docker compose down failed: {err}");
            return false;
        }
        return true;
    }

    public async Task<bool> ComposeStartAsync()
    {
        var (code, _, err) = await platform.RunAsync(
            "docker", "compose start", workDir: InstallPaths.InstallDir);

        if (code != 0)
        {
            Display.Error($"docker compose start failed: {err}");
            return false;
        }
        return true;
    }

    public async Task<bool> ComposeStopAsync()
    {
        var (code, _, err) = await platform.RunAsync(
            "docker", "compose stop", workDir: InstallPaths.InstallDir);

        if (code != 0)
        {
            Display.Error($"docker compose stop failed: {err}");
            return false;
        }
        return true;
    }

    public async Task<bool> IsAppRunningAsync()
    {
        var (_, output, _) = await platform.RunAsync(
            "docker", "inspect -f {{.State.Running}} authvault-app");
        return output.Trim() == "true";
    }

    public async Task<string> ComposeStatusAsync()
    {
        var (_, output, _) = await platform.RunAsync(
            "docker", "compose ps", workDir: InstallPaths.InstallDir);
        return output;
    }

    // Docker installation (Linux only) 

    async Task<bool> InstallDockerLinuxAsync()
    {
        if (!Display.Confirm("Install Docker automatically?")) return false;

        bool ok = await Display.SpinnerAsync("Installing Docker...", async () =>
        {
            var (dlCode, _, _) = await platform.RunAsync(
                "sh", "-c \"curl -fsSL https://get.docker.com | sh\"", sudo: false);
            if (dlCode != 0) return false;

            var user = Environment.UserName;
            await platform.RunAsync("usermod", $"-aG docker {user}", sudo: true);
            return true;
        });

        if (!ok)
        {
            Display.Error("Docker installation failed. Install manually:");
            Display.Link("https://docs.docker.com/engine/install/");
            return false;
        }

        Display.Success("Docker installed");
        Display.Warning("You may need to log out and back in for docker group permissions to take effect.");
        await platform.RunAsync("systemctl", "enable --now docker", sudo: true);

        return await IsDockerAvailableAsync();
    }

    // Diagnostics

    public async Task DiagnoseFailureAsync()
    {
        var (_, logs, _) = await platform.RunAsync("docker", "logs authvault-db");

        if (logs.Contains("Password did not match") || logs.Contains("18456"))
        {
            Display.Warning("The database rejected the SA password.");
            Display.Info("This happens when the Docker volume already contains a database");
            Display.Info("initialized with a different password.\n");
            Display.Info("To fix, remove the old data and reinstall:");
            Display.Info("  [bold]docker rm -f authvault-app authvault-db[/]");
            Display.Info("  [bold]docker volume rm authvault-data[/]");
            Display.Info("  [bold]authvault install[/]");
            Display.Info("\n[yellow]Warning:[/] this will delete all saved passwords.");
        }
    }

    // Helpers

    async Task<bool> IsDockerAvailableAsync()
    {
        var (code, _, _) = await platform.RunAsync("docker", "info");
        return code == 0;
    }

    async Task<bool> IsComposeAvailableAsync()
    {
        var (code, _, _) = await platform.RunAsync("docker", "compose version");
        return code == 0;
    }
}
