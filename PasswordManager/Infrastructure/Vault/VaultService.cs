using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Vault;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;
using PasswordManager.ViewModels.Vault;
using PasswordManager.ViewModels.Vault.VaultItems;

namespace PasswordManager.Infrastructure.Vault
{
    public class VaultService : IVaultHomeService, IVaultSidebarService
    {
        private readonly AppDbContext _db;

        public VaultService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<VaultHomeViewModel> GetHomeDataAsync(int userId)
        {
            var items = await GetItemsFromDBAsync(userId);
            var model = new VaultHomeViewModel
            {
                Sidebar = await GetSidebarDataAsync(userId),
                Items = items,
            };
            return model;
        }

        public async Task<VaultSidebarViewModel> GetSidebarDataAsync(int userId)
        {
            var countItems =
                await _db.LoginData.CountAsync(x => x.UserId == userId)
              + await _db.CardData.CountAsync(x => x.UserId == userId)
              + await _db.NoteData.CountAsync(x => x.UserId == userId);

            return new VaultSidebarViewModel
            {
                UserId = userId,
                UserName = "AuthVault",
                CountAllItems = countItems
            };
        }

        public async Task<List<VaultItemViewModel>> GetItemsFromDBAsync(int userId)
        {
            var items = new List<VaultItemViewModel>();

            // LOGIN ITEMS
            var loginItems = await _db.LoginData
                .Where(x => x.UserId == userId)
                .Select(x => new LoginItemViewModel
                {
                    Id = x.Id,
                    FolderId = x.FolderId,
                    Title = x.Title,
                    CreatedAt = x.CreatedAt,
                    Login = x.Login,
                    Password = x.PasswordEncrypted,
                    Note = x.NoteEncrypted != null
                        ? x.NoteEncrypted.Split('\n', StringSplitOptions.None).ToList()
                        : new List<string>()

                })
                .ToListAsync();

            // CARD ITEMS
            var cardItems = await _db.CardData
                .Where(x => x.UserId == userId)
                .Select(x => new CardItemViewModel
                {
                    Id = x.Id,
                    FolderId = x.FolderId,
                    Title = x.Title,
                    CreatedAt = x.CreatedAt,
                    CardNumber = x.CardNumberEncrypted,
                    ExpireMonth = x.ExpireMonthEncrypted,
                    ExpireYear = x.ExpireYearEncrypted,
                    Note = x.NoteEncrypted != null
                        ? x.NoteEncrypted.Split('\n', StringSplitOptions.None).ToList()
                        : new List<string>()
                })
                .ToListAsync();

            // NOTE ITEMS
            var noteItems = await _db.NoteData
                .Where(x => x.UserId == userId)
                .Select(x => new NoteItemViewModel
                {
                    Id = x.Id,
                    FolderId = x.FolderId,
                    Title = x.Title,
                    CreatedAt = x.CreatedAt,
                    Content = x.NoteEncrypted != null
                        ? x.NoteEncrypted.Split('\n', StringSplitOptions.None).ToList()
                        : new List<string>()
                })
                .ToListAsync();

            items.AddRange(loginItems);
            items.AddRange(cardItems);
            items.AddRange(noteItems);

            return items;
        }


        public async Task SeedTestDataAsync(int userId)
        {
            if (await _db.LoginData.AnyAsync(x => x.UserId == userId))
                return;

            var folder = new Folder
            {
                UserId = userId,
                Name = "Default",
                CreatedAt = DateTime.UtcNow
            };

            _db.Folders.Add(folder);

            _db.LoginData.Add(new LoginData
            {
                UserId = userId,
                Title = "Google",
                Folder = folder,
                Login = "user@gmail.com",
                PasswordEncrypted = "pass123",
                NoteEncrypted = "This is my Google account",
                CreatedAt = DateTime.UtcNow
            });

            _db.CardData.Add(new CardData
            {
                UserId = userId,
                Title = "Visa",
                Folder = folder,
                CardNumberEncrypted = "1234567812345678",
                ExpireMonthEncrypted = "12",
                ExpireYearEncrypted = "28",
                NoteEncrypted = "My primary credit card",
                CreatedAt = DateTime.UtcNow
            });

            _db.NoteData.Add(new NoteData
            {
                UserId = userId,
                Folder = folder,
                Title = "Private note",
                NoteEncrypted = "Hello\nThis is a test note",
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }

    }
}
