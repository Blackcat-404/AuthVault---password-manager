namespace PasswordManager.Application.Vault
{
    public interface IDeleteFolderService
    {
        Task DeleteFolderAsync(int userId, int folderId);
    }
}
