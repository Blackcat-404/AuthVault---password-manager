using Microsoft.AspNetCore.Mvc;

namespace PasswordManager.Application.Vault
{
    public interface IUpdateItemFieldService
    {
        Task UpdateLoginFieldAsync(int userId, int itemId, string fieldName, string fieldValue);
        Task UpdateCardFieldAsync(int userId, int itemId, string fieldName, string fieldValue);
        Task UpdateNoteFieldAsync(int userId, int itemId, string fieldName, string fieldValue);
    }
}
