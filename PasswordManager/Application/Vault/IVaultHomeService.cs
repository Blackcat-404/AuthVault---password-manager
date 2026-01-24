using PasswordManager.ViewModels.Vault;
using PasswordManager.ViewModels.Vault.VaultItems;

namespace PasswordManager.Application.Vault
{
    public interface IVaultHomeService
    {
        Task<VaultHomeViewModel> GetHomeDataAsync(int userId);
        Task<List<VaultItemViewModel>> GetItemsFromDBAsync(int userId);
        Task SeedTestDataAsync(int userId);
    }
}