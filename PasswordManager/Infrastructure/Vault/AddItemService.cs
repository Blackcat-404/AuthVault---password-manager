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
            await _db.LoginData.AddAsync(new LoginData
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
            await _db.CardData.AddAsync(new CardData
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
            await _db.NoteData.AddAsync(new NoteData
            {
                Title = dto.Title!,
                NoteEncrypted = dto.Content, //TODO:Encrypt
                UserId = dto.UserId,
                FolderId = dto.FolderId,
                CreatedAt = DateTime.UtcNow,
            });

            await _db.SaveChangesAsync();
        }
    }
}
