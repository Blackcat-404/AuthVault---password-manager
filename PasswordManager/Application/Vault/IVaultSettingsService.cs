using PasswordManager.ViewModels.Vault;

namespace PasswordManager.Application.Vault
{
    public interface IVaultSettingsService
    {
        Task<VaultSettingsViewModel> GetSettingsDataAsync(int userId);
    }
}
