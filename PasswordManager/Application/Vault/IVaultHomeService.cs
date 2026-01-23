using PasswordManager.ViewModels.Vault;

namespace PasswordManager.Application.Vault
{
    public interface IVaultHomeService
    {
        Task<VaultHomeViewModel> GetHomeDataAsync(int userId);
    }
}
