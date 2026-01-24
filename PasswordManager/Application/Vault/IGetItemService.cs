using PasswordManager.Domain.Entities;
using PasswordManager.ViewModels.Vault.VaultItems;

namespace PasswordManager.Application.Vault
{
    public interface IGetItemService
    {
        Task<VaultItemViewModel> GetLoginItemAsync(int userId, int itemId);
        Task<VaultItemViewModel> GetCardItemAsync(int userId, int itemId);
        Task<VaultItemViewModel> GetNoteItemAsync(int userId, int itemId);
    }
}
