namespace PasswordManager.Application.Vault
{
    public interface IAddItemService
    {
        Task AddLoginAsync(LoginItemDto dto);
        Task AddCardAsync(CardItemDto dto);
        Task AddNoteAsync(NoteItemDto dto);
    }
}
