using PasswordManager.ViewModels.Vault;

namespace PasswordManager.Application.Vault
{
    public interface IVaultSettingsService
    {
        Task<VaultSettingsViewModel> GetSettingsDataAsync(int userId);
        Task<FAuthenticationCodeViewModel> Get2FACodeAsync(int userId);
        Task<FAuthenticationEmailViewModel> Get2FAEmailAsync(int userId);
    }
}
