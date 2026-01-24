using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Vault;
using PasswordManager.Data;

namespace PasswordManager.Infrastructure.Vault
{
    public class UpdateItemFieldService : IUpdateItemFieldService
    {

        private readonly AppDbContext _db;

        public UpdateItemFieldService(AppDbContext db)
        {
            _db = db;
        }

        public async Task UpdateLoginFieldAsync(int userId, int itemId, string fieldName, string fieldValue)
        {

            var loginItem = await _db.LoginData
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == itemId);


            switch (fieldName)
            {
                case "Title":
                    loginItem!.Title = fieldValue ??= string.Empty;
                    break;
                case "Login":
                    loginItem!.Login = fieldValue ??= string.Empty;
                    break;
                case "Password":
                    loginItem!.PasswordEncrypted = fieldValue ??= string.Empty;
                    break;
                case "WebURL":
                    // loginItem.WebURL = fieldValue;
                    break;
                case "Note":
                    loginItem!.NoteEncrypted = fieldValue ??= string.Empty;
                    break;
            }

            await _db.SaveChangesAsync();

        }


        public async Task UpdateCardFieldAsync(int userId, int itemId, string fieldName, string fieldValue)
        {

            var cardItem = await _db.CardData
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == itemId);

            switch (fieldName)
            {
                case "Title":
                    cardItem!.Title = fieldValue ??= string.Empty;
                    break;
                case "CardNumber":

                    cardItem!.CardNumberEncrypted = fieldValue ??= string.Empty;
                    break;
                case "ExpireMonth":
                    cardItem!.ExpireMonthEncrypted = fieldValue;
                    break;
                case "ExpireYear":
                    cardItem!.ExpireYearEncrypted = fieldValue;
                    break;
                case "Note":
                    cardItem!.NoteEncrypted = fieldValue ??= string.Empty;
                    break;

            }
            await _db.SaveChangesAsync();
        }


        public async Task UpdateNoteFieldAsync(int userId, int itemId, string fieldName, string fieldValue)
        {

            var noteItem = await _db.NoteData
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == itemId);

            switch (fieldName)
            {
                case "Title":
                    noteItem!.Title = fieldValue ??= string.Empty;
                    break;
                case "Content":
                    noteItem!.NoteEncrypted = fieldValue ??= string.Empty;
                    break;
            }

            await _db.SaveChangesAsync();
        }
    }
}
