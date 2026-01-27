using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Vault;
using PasswordManager.Data;
using PasswordManager.ViewModels.Vault;
using PasswordManager.ViewModels.Vault.VaultItems;

namespace PasswordManager.Infrastructure.Vault
{
    public class VaultService : IVaultHomeService, IVaultSidebarService, IVaultSettingsService
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

            var name = await _db.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Login)
                .FirstOrDefaultAsync();

            return new VaultSidebarViewModel
            {
                UserId = userId,
                UserName = name,
                CountAllItems = countItems
            };
        }

        public async Task<VaultSettingsViewModel> GetSettingsDataAsync(int userId)
        {
            var email = await _db.Users
                .Where(u => userId == u.Id)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
            var accountCreatedAt = (await _db.Users.FindAsync(userId))!.CreatedAt;
            var user = await _db.TwoFactorAuthentications
                .FirstOrDefaultAsync(u => userId == u.UserId);

            var model = new VaultSettingsViewModel
            {
                Sidebar = await GetSidebarDataAsync(userId),
                Email = email!,
                FAEmail = user!.Email,
                Is2FAEnabled = user!.IsEnabled,
                accountCreatedOn = accountCreatedAt.ToString("MMMM dd, yyyy")
            };
            return model;
        }

        public async Task<FAuthenticationEmailViewModel> Get2FAEmailAsync(int userId)
        {
            var email = await _db.Users
                .Where(u => userId == u.Id)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

            var model = new FAuthenticationEmailViewModel
            {
                Sidebar = await GetSidebarDataAsync(userId),
            };
            return model;
        }

        public async Task<FAuthenticationCodeViewModel> Get2FACodeAsync(int userId)
        {
            var email = await _db.Users
                .Where(u => userId == u.Id)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

            var model = new FAuthenticationCodeViewModel
            {
                Sidebar = await GetSidebarDataAsync(userId),
            };
            return model;
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
                    WebURL = "google.com",
                    Login = x.LoginEncrypted,
                    Password = x.PasswordEncrypted,
                    Note = x.NoteEncrypted

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
                    Note = x.NoteEncrypted
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
                    Content = x.NoteEncrypted
                })
                .ToListAsync();

            items.AddRange(loginItems);
            items.AddRange(cardItems);
            items.AddRange(noteItems);

            return items;
        }
    }
}