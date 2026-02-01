using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Vault;
using PasswordManager.Data;
using PasswordManager.Infrastructure.Security;
using PasswordManager.Application.Security;

namespace PasswordManager.Infrastructure.Vault
{

    public class UpdateItemFieldService : IUpdateItemFieldService
    {
        private readonly AppDbContext _db;
        private readonly IEncryptionService _encryptionService;
        private readonly ISessionEncryptionService _sessionEncryptionService;

        public UpdateItemFieldService(AppDbContext db,
                                        IEncryptionService encryptionService,
                                        ISessionEncryptionService sessionEncryptionService)
        {
            _db = db;
            _encryptionService = encryptionService;
            _sessionEncryptionService = sessionEncryptionService;
        }

        public async Task UpdateLoginFieldAsync(int userId, int itemId, string fieldName, string fieldValue)
        {
            byte[]? encryptionKey = _sessionEncryptionService.GetEncryptionKey(userId);

            if (encryptionKey == null)
            {
                throw new InvalidOperationException("Encryption key doesn't found");
            }

            var loginItem = await _db.LoginData
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == itemId);

            if (loginItem == null)
                return;

            switch (fieldName)
            {
                case "Title":
                    loginItem.Title = fieldValue ?? string.Empty;
                    break;

                case "FolderId":
                    loginItem.FolderId = string.IsNullOrEmpty(fieldValue)
                        ? (int?)null
                        : Convert.ToInt32(fieldValue);
                    break;

                case "Login":
                    var encryptedLogin = _encryptionService.Encrypt(fieldValue ?? string.Empty, encryptionKey);
                    loginItem.LoginEncrypted = encryptedLogin.EncryptedData;
                    loginItem.LoginIV = encryptedLogin.IV;
                    break;

                case "Password":
                    var encryptedPassword = _encryptionService.Encrypt(fieldValue ?? string.Empty, encryptionKey);
                    loginItem.PasswordEncrypted = encryptedPassword.EncryptedData;
                    loginItem.PasswordIV = encryptedPassword.IV;
                    break;

                case "WebURL":
                    loginItem.WebURL = fieldValue ?? string.Empty;
                    break;

                case "Note":
                    if (string.IsNullOrEmpty(fieldValue))
                    {
                        loginItem.NoteEncrypted = null;
                        loginItem.NoteIV = null;
                    }
                    else
                    {
                        var encryptedNote = _encryptionService.Encrypt(fieldValue, encryptionKey);
                        loginItem.NoteEncrypted = encryptedNote.EncryptedData;
                        loginItem.NoteIV = encryptedNote.IV;
                    }
                    break;
            }

            await _db.SaveChangesAsync();
        }

        public async Task UpdateCardFieldAsync(int userId, int itemId, string fieldName, string fieldValue)
        {
            byte[]? encryptionKey = _sessionEncryptionService.GetEncryptionKey(userId);

            if (encryptionKey == null)
            {
                throw new InvalidOperationException("Encryption key doesn't found");
            }

            var cardItem = await _db.CardData
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == itemId);

            if (cardItem == null)
                return;

            switch (fieldName)
            {
                case "Title":
                    cardItem.Title = fieldValue ?? string.Empty;
                    break;

                case "FolderId":
                    cardItem.FolderId = string.IsNullOrEmpty(fieldValue)
                        ? (int?)null
                        : Convert.ToInt32(fieldValue);
                    break;

                case "CardNumber":
                    var encryptedCardNumber = _encryptionService.Encrypt(fieldValue ?? string.Empty, encryptionKey);
                    cardItem.CardNumberEncrypted = encryptedCardNumber.EncryptedData;
                    cardItem.CardNumberIV = encryptedCardNumber.IV;
                    break;

                case "CardholderName":
                    cardItem.CardholderName = fieldValue ?? string.Empty;
                    break;

                case "ExpireMonth":
                    var encryptedMonth = _encryptionService.Encrypt(fieldValue ?? string.Empty, encryptionKey);
                    cardItem.ExpireMonthEncrypted = encryptedMonth.EncryptedData;
                    cardItem.ExpireMonthIV = encryptedMonth.IV;
                    break;

                case "ExpireYear":
                    var encryptedYear = _encryptionService.Encrypt(fieldValue ?? string.Empty, encryptionKey);
                    cardItem.ExpireYearEncrypted = encryptedYear.EncryptedData;
                    cardItem.ExpireYearIV = encryptedYear.IV;
                    break;

                case "Note":
                    if (string.IsNullOrEmpty(fieldValue))
                    {
                        cardItem.NoteEncrypted = null;
                        cardItem.NoteIV = null;
                    }
                    else
                    {
                        var encryptedNote = _encryptionService.Encrypt(fieldValue, encryptionKey);
                        cardItem.NoteEncrypted = encryptedNote.EncryptedData;
                        cardItem.NoteIV = encryptedNote.IV;
                    }
                    break;
            }

            await _db.SaveChangesAsync();
        }

        public async Task UpdateNoteFieldAsync(int userId, int itemId, string fieldName, string fieldValue)
        {
            byte[]? encryptionKey = _sessionEncryptionService.GetEncryptionKey(userId);

            if (encryptionKey == null)
            {
                throw new InvalidOperationException("Encryption key doesn't found");
            }

            var noteItem = await _db.NoteData
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == itemId);

            if (noteItem == null)
                return;

            switch (fieldName)
            {
                case "Title":
                    noteItem.Title = fieldValue ?? string.Empty;
                    break;

                case "FolderId":
                    noteItem.FolderId = string.IsNullOrEmpty(fieldValue)
                        ? (int?)null
                        : Convert.ToInt32(fieldValue);
                    break;

                case "Content":
                    var encryptedContent = _encryptionService.Encrypt(fieldValue ?? string.Empty, encryptionKey);
                    noteItem.NoteEncrypted = encryptedContent.EncryptedData;
                    noteItem.NoteIV = encryptedContent.IV;
                    break;
            }

            await _db.SaveChangesAsync();
        }
    }
}