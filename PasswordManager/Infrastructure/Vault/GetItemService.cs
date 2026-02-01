using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Vault;
using PasswordManager.Data;
using PasswordManager.Infrastructure.Security;
using PasswordManager.ViewModels.Vault.VaultItems;
using PasswordManager.Application.Security;

namespace PasswordManager.Infrastructure.Vault
{

    public class GetItemService : IGetItemService
    {
        private readonly AppDbContext _db;
        private readonly IEncryptionService _encryptionService;
        private readonly ISessionEncryptionService _sessionEncryptionService;

        public GetItemService(
            AppDbContext db,
            IEncryptionService encryptionService,
            ISessionEncryptionService sessionEncryptionService)
        {
            _db = db;
            _encryptionService = encryptionService;
            _sessionEncryptionService = sessionEncryptionService;
        }

        public async Task<VaultItemViewModel> GetLoginItemAsync(int userId, int itemId)
        {
            byte[]? encryptionKey = _sessionEncryptionService.GetEncryptionKey(userId);

            if (encryptionKey == null)
            {
                throw new InvalidOperationException("Encryption key doesn't found");
            }

            var loginItem = await _db.LoginData
                .Where(x => userId == x.UserId && itemId == x.Id)
                .Select(l => new
                {
                    l.Id,
                    l.FolderId,
                    FolderName = l.Folder!.Name,
                    l.Title,
                    l.CreatedAt,
                    l.WebURL,

                    l.LoginEncrypted,
                    l.LoginIV,
                    l.PasswordEncrypted,
                    l.PasswordIV,
                    l.NoteEncrypted,
                    l.NoteIV
                })
                .FirstOrDefaultAsync();

            if (loginItem == null)
                return null!;

            string decryptedLogin = _encryptionService.Decrypt(
                loginItem.LoginEncrypted!,
                loginItem.LoginIV!,
                encryptionKey);

            string decryptedPassword = _encryptionService.Decrypt(
                loginItem.PasswordEncrypted!,
                loginItem.PasswordIV!,
                encryptionKey);

            string? decryptedNote = string.IsNullOrEmpty(loginItem.NoteEncrypted)
                ? null
                : _encryptionService.Decrypt(
                    loginItem.NoteEncrypted,
                    loginItem.NoteIV!,
                    encryptionKey);

            return new LoginItemViewModel
            {
                Id = loginItem.Id,
                FolderId = loginItem.FolderId,
                FolderName = loginItem.FolderName,
                Title = loginItem.Title,
                CreatedAt = loginItem.CreatedAt,
                WebURL = loginItem.WebURL,
                Login = decryptedLogin,
                Password = decryptedPassword,
                Note = decryptedNote
            };
        }

        public async Task<VaultItemViewModel> GetCardItemAsync(int userId, int itemId)
        {
            byte[]? encryptionKey = _sessionEncryptionService.GetEncryptionKey(userId);

            if (encryptionKey == null)
            {
                throw new InvalidOperationException("Encryption key doesn't found");
            }

            var cardItem = await _db.CardData
                .Where(x => userId == x.UserId && itemId == x.Id)
                .Select(c => new
                {
                    c.Id,
                    c.FolderId,
                    FolderName = c.Folder!.Name,
                    c.Title,
                    c.CreatedAt,
                    c.CardholderName,
                    c.CardNumberEncrypted,
                    c.CardNumberIV,
                    c.ExpireMonthEncrypted,
                    c.ExpireMonthIV,
                    c.ExpireYearEncrypted,
                    c.ExpireYearIV,
                    c.NoteEncrypted,
                    c.NoteIV
                })
                .FirstOrDefaultAsync();

            if (cardItem == null)
                return null!;

            string decryptedCardNumber = _encryptionService.Decrypt(
                cardItem.CardNumberEncrypted!,
                cardItem.CardNumberIV!,
                encryptionKey);

            string decryptedExpireMonth = _encryptionService.Decrypt(
                cardItem.ExpireMonthEncrypted!,
                cardItem.ExpireMonthIV!,
                encryptionKey);

            string decryptedExpireYear = _encryptionService.Decrypt(
                cardItem.ExpireYearEncrypted!,
                cardItem.ExpireYearIV!,
                encryptionKey);

            string? decryptedNote = string.IsNullOrEmpty(cardItem.NoteEncrypted)
                ? null
                : _encryptionService.Decrypt(
                    cardItem.NoteEncrypted,
                    cardItem.NoteIV!,
                    encryptionKey);

            return new CardItemViewModel
            {
                Id = cardItem.Id,
                FolderId = cardItem.FolderId,
                FolderName = cardItem.FolderName,
                Title = cardItem.Title,
                CreatedAt = cardItem.CreatedAt,
                CardholderName = cardItem.CardholderName,
                CardNumber = decryptedCardNumber,
                ExpireMonth = decryptedExpireMonth,
                ExpireYear = decryptedExpireYear,
                Note = decryptedNote
            };
        }

        public async Task<VaultItemViewModel> GetNoteItemAsync(int userId, int itemId)
        {
            byte[]? encryptionKey = _sessionEncryptionService.GetEncryptionKey(userId);

            if (encryptionKey == null)
            {
                throw new InvalidOperationException("Encryption key doesn't found");
            }

            var noteItem = await _db.NoteData
                .Where(x => userId == x.UserId && itemId == x.Id)
                .Select(n => new
                {
                    n.Id,
                    n.FolderId,
                    FolderName = n.Folder!.Name,
                    n.Title,
                    n.CreatedAt,
                    n.NoteEncrypted,
                    n.NoteIV
                })
                .FirstOrDefaultAsync();

            if (noteItem == null)
                return null!;

            string decryptedContent = _encryptionService.Decrypt(
                noteItem.NoteEncrypted!,
                noteItem.NoteIV!,
                encryptionKey);

            return new NoteItemViewModel
            {
                Id = noteItem.Id,
                FolderId = noteItem.FolderId,
                FolderName = noteItem.FolderName,
                Title = noteItem.Title,
                CreatedAt = noteItem.CreatedAt,
                Content = decryptedContent
            };
        }
    }
}