using PasswordManager.Application.Vault;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;
using PasswordManager.Infrastructure.Security;
using PasswordManager.Application.Security;

namespace PasswordManager.Infrastructure.Vault
{
    public class AddItemService : IAddItemService
    {
        private readonly AppDbContext _db;
        private readonly IEncryptionService _encryptionService;
        private readonly ISessionEncryptionService _sessionEncryptionService;

        public AddItemService(AppDbContext db,
            IEncryptionService encryptionService,
            ISessionEncryptionService sessionEncryptionService)
        {
            _db = db;
            _encryptionService = encryptionService;
            _sessionEncryptionService = sessionEncryptionService;
        }

        public async Task AddLoginAsync(LoginItemDto dto)
        {
            byte[]? encryptionKey = _sessionEncryptionService.GetEncryptionKey(dto.UserId);

            if (encryptionKey == null)
            {
                throw new InvalidOperationException("Encryption key doesn't found");
            }

            var encryptedLogin = _encryptionService.Encrypt(dto.Login, encryptionKey);
            var encryptedPassword = _encryptionService.Encrypt(dto.Password!, encryptionKey);
            var encryptedNote = string.IsNullOrEmpty(dto.Note)
                ? new EncryptionResult { EncryptedData = "", IV = "" }
                : _encryptionService.Encrypt(dto.Note, encryptionKey);

            var loginItem = new LoginData
            {
                UserId = dto.UserId,
                FolderId = dto.FolderId,
                Title = dto.Title!,
                WebURL = dto.WebURL,

                LoginEncrypted = encryptedLogin.EncryptedData,
                LoginIV = encryptedLogin.IV,

                PasswordEncrypted = encryptedPassword.EncryptedData,
                PasswordIV = encryptedPassword.IV,

                NoteEncrypted = encryptedNote.EncryptedData,
                NoteIV = encryptedNote.IV,

                CreatedAt = dto.CreatedAt
            };

            await _db.LoginData.AddAsync(loginItem);
            await _db.SaveChangesAsync();
        }

        public async Task AddCardAsync(CardItemDto dto)
        {
            byte[]? encryptionKey = _sessionEncryptionService.GetEncryptionKey(dto.UserId);

            if (encryptionKey == null)
            {
                throw new InvalidOperationException("Encryption key doesn't found");
            }

            var encryptedCardNumber = _encryptionService.Encrypt(dto.CardNumber!, encryptionKey);
            var encryptedExpireMonth = _encryptionService.Encrypt(dto.ExpireMonth!, encryptionKey);
            var encryptedExpireYear = _encryptionService.Encrypt(dto.ExpireYear!, encryptionKey);
            var encryptedNote = string.IsNullOrEmpty(dto.Note)
                ? new EncryptionResult { EncryptedData = "", IV = "" }
                : _encryptionService.Encrypt(dto.Note, encryptionKey);

            var cardItem = new CardData
            {
                UserId = dto.UserId,
                FolderId = dto.FolderId,
                Title = dto.Title!,
                CardholderName = dto.CardholderName,

                CardNumberEncrypted = encryptedCardNumber.EncryptedData,
                CardNumberIV = encryptedCardNumber.IV,

                ExpireMonthEncrypted = encryptedExpireMonth.EncryptedData,
                ExpireMonthIV = encryptedExpireMonth.IV,

                ExpireYearEncrypted = encryptedExpireYear.EncryptedData,
                ExpireYearIV = encryptedExpireYear.IV,

                NoteEncrypted = encryptedNote.EncryptedData,
                NoteIV = encryptedNote.IV,

                CreatedAt = dto.CreatedAt
            };

            await _db.CardData.AddAsync(cardItem);
            await _db.SaveChangesAsync();
        }

        public async Task AddNoteAsync(NoteItemDto dto)
        {
            byte[]? encryptionKey = _sessionEncryptionService.GetEncryptionKey(dto.UserId);

            if (encryptionKey == null)
            {
                throw new InvalidOperationException("Encryption key doesn't found");
            }

            var encryptedContent = _encryptionService.Encrypt(dto.Content!, encryptionKey);

            var noteItem = new NoteData
            {
                UserId = dto.UserId,
                FolderId = dto.FolderId,
                Title = dto.Title!,

                NoteEncrypted = encryptedContent.EncryptedData,
                NoteIV = encryptedContent.IV,

                CreatedAt = dto.CreatedAt
            };

            await _db.NoteData.AddAsync(noteItem);
            await _db.SaveChangesAsync();
        }
    }
}
