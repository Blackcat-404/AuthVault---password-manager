using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Vault;
using PasswordManager.Data;
using PasswordManager.ViewModels.Vault.VaultItems;

namespace PasswordManager.Infrastructure.Vault
{
    public class GetItemService : IGetItemService
    {
        private readonly AppDbContext _db;

        public GetItemService(AppDbContext db)
        {
            _db = db;
        }

            public async Task<VaultItemViewModel> GetLoginItemAsync(int userId, int itemId)
        {
            var loginItem = await _db.LoginData
                .Where(x => userId == x.UserId && itemId == x.Id)
                .Select(l => new LoginItemViewModel
                {
                    Id = l!.Id,
                    FolderId = l.FolderId,
                    FolderName = l.Folder!.Name,
                    Title = l.Title,
                    CreatedAt = l.CreatedAt,
                    Login = l.LoginEncrypted,
                    Password = l.PasswordEncrypted,
                    Note = l.NoteEncrypted,
                    WebURL = l.WebURL,
                })
                .FirstOrDefaultAsync();

            return loginItem!;
        }

        public async Task<VaultItemViewModel> GetCardItemAsync(int userId, int itemId)
        {

            var cardItem = await _db.CardData
                .Where(x => userId == x.UserId && itemId == x.Id)
                .Select(c => new CardItemViewModel
                {
                    Id = c.Id,
                    FolderId = c.FolderId,
                    FolderName = c.Folder!.Name,
                    Title = c.Title,
                    CreatedAt = c.CreatedAt,
                    CardNumber = c.CardNumberEncrypted,
                    ExpireMonth = c.ExpireMonthEncrypted,
                    ExpireYear = c.ExpireYearEncrypted,
                    CardholderName = c.CardholderName,
                    Note = c.NoteEncrypted
                })
                .FirstOrDefaultAsync();

            return cardItem!;
        }

        public async Task<VaultItemViewModel> GetNoteItemAsync(int userId, int itemId)
        {

            var noteItem = await _db.NoteData
                .Where(x => userId == x.UserId && itemId == x.Id)
                .Select(n => new NoteItemViewModel
                {
                    Id = n!.Id,
                    FolderId = n.FolderId,
                    FolderName = n.Folder!.Name,
                    Title = n.Title,
                    CreatedAt = n.CreatedAt,
                    Content = n.NoteEncrypted,
                })
                .FirstOrDefaultAsync();

            return noteItem!;
        }
    }
}
