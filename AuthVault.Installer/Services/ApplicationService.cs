using AuthVault.Installer.UI;

namespace AuthVault.Installer.Services;

public class ApplicationService(DockerService docker)
{
    public async Task<int> StartAsync()
    {
        if (await docker.IsAppRunningAsync())
        {
            Display.Warning("AuthVault is already running.");
            return 0;
        }

        Display.Step("Starting AuthVault containers...");
        bool ok = await docker.ComposeStartAsync();

        if (ok) Display.Success("AuthVault started.");
        return ok ? 0 : 1;
    }

    public async Task<int> StopAsync()
    {
        if (!await docker.IsAppRunningAsync())
        {
            Display.Info("AuthVault is not running.");
            return 0;
        }

        Display.Step("Stopping AuthVault containers...");
        bool ok = await docker.ComposeStopAsync();

        if (ok) Display.Success("AuthVault stopped.");
        return ok ? 0 : 1;
    }

    public async Task<int> StatusAsync()
    {
        Display.Section("AuthVault Status");

        var status = await docker.ComposeStatusAsync();
        if (string.IsNullOrWhiteSpace(status))
        {
            Display.Warning("No containers found. Run [bold]authvault install[/] first.");
        }
        else
        {
            Display.Info(status);
        }

        return 0;
    }
}
