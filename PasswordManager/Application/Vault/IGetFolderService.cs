using PasswordManager.ViewModels.Vault;

namespace PasswordManager.Application.Vault
{
    public interface IGetFolderService
    {
        Task<VaultHomeViewModel> GetFolderAsync(int userId, int folderId);
    }
}
