namespace PasswordManager.Application.Vault
{
    public interface IDeleteItemService
    {
        Task DeleteLoginItemAsync(int userId, int itemId);
        Task DeleteCardItemAsync(int userId, int itemId);
        Task DeleteNoteItemAsync(int userId, int itemId);
    }
}
