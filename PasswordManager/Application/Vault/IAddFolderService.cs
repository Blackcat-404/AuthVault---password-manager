namespace PasswordManager.Application.Vault
{
    public interface IAddFolderService
    {
        Task AddFolderAsync(FolderDto dto);
    }
}
