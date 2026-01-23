using PasswordManager.Data;
using PasswordManager.Application.Vault;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Infrastructure.Vault
{
    public class AddItemService : IAddItemService
    {
        private readonly AppDbContext _db;
        public AddItemService(AppDbContext db) 
        {
            _db = db;
        }

        public async Task AddLoginAsync(LoginItemDto dto)
        {
            _db.LoginData.Add(new LoginData
            {
                Title = dto.Title!,
                PasswordEncrypted = dto.Password, //TODO:Encrypt password
                NoteEncrypted = dto.Note, //TODO:Encrypt note
                UserId = dto.UserId,
                FolderId = dto.FolderId,
                CreatedAt = dto.CreatedAt,
                LoginEncrypted = dto.Login, //TODO:Encrypt login
                WebURL = dto.WebURL
            });

            await _db.SaveChangesAsync();
        }

        public async Task AddCardAsync(CardItemDto dto)
        {
            _db.CardData.Add(new CardData
            {
                Title = dto.Title!,
                CardNumberEncrypted = dto.CardNumber, //TODO:Encrypt
                CardholderName = dto.CardholderName, 
                ExpireMonthEncrypted = dto.ExpireMonth, //TODO:Encrypt
                ExpireYearEncrypted = dto.ExpireYear, //TODO:Encrypt
                NoteEncrypted = dto.Note, //TODO:Encrypt
                UserId = dto.UserId,
                FolderId = dto.FolderId,
                CreatedAt = DateTime.UtcNow,
            });

            await _db.SaveChangesAsync();
        }

        public async Task AddNoteAsync(NoteItemDto dto)
        {
            _db.NoteData.Add(new NoteData
            {
                Title = dto.Title!,
                NoteEncrypted = dto.Note, //TODO:Encrypt
                UserId = dto.UserId,
                FolderId = dto.FolderId,
                CreatedAt = DateTime.UtcNow,
            });

            await _db.SaveChangesAsync();
        }
    }
}
