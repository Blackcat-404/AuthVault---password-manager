using PasswordManager.ViewModels.Vault;

namespace PasswordManager.Application.Vault
{
    public interface IVaultSidebarService
    {
        Task<VaultSidebarViewModel> GetSidebarDataAsync(int userId);
    }
}
