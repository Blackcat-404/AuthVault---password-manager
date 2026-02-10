using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Vault;
using PasswordManager.Data;
using PasswordManager.Infrastructure.Security;
using PasswordManager.Domain.Entities;
using PasswordManager.ViewModels.Vault;
using PasswordManager.ViewModels.Vault.VaultItems;
using PasswordManager.Application.Security;

namespace PasswordManager.Infrastructure.Vault
{

    public class VaultService : IVaultHomeService, IVaultSidebarService, IVaultSettingsService
    {
        private readonly AppDbContext _db;
        private readonly IEncryptionService _encryptionService;
        private readonly ISessionEncryptionService _sessionEncryptionService;

        public VaultService(
            AppDbContext db,
            IEncryptionService encryptionService,
            ISessionEncryptionService sessionEncryptionService)
        {
            _db = db;
            _encryptionService = encryptionService;
            _sessionEncryptionService = sessionEncryptionService;
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

            var folders = await _db.Folders
                .Where(f => f.UserId == userId)
                .Select(f => new FolderViewModel
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    Color = f.Color,
                    CreatedAt = f.CreatedAt
                })
                .OrderBy(f => f.Name)
                .ToListAsync();

            return new VaultSidebarViewModel
            {
                UserId = userId,
                UserName = name,
                CountAllItems = countItems,
                Folders = folders
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
                FAEmail = (user == null) ? null : user.Email,
                Is2FAEnabled = (user == null) ? false : user.IsEnabled,
                accountCreatedOn = accountCreatedAt.ToString("MMMM dd, yyyy"),
                PasswordLastChangeAt = (user == null) ? null : user.User.PasswordLastChangedAt
            };
            return model;
        }

        public async Task<FAuthenticationEmailViewModel> Get2FAEmailAsync(int userId)
        {
            var model = new FAuthenticationEmailViewModel
            {
                Sidebar = await GetSidebarDataAsync(userId),
            };
            return model;
        }

        public async Task<FAuthenticationCodeViewModel> Get2FACodeAsync(int userId)
        {
            var model = new FAuthenticationCodeViewModel
            {
                Sidebar = await GetSidebarDataAsync(userId),
            };
            return model;
        }

        public async Task<ChangeMasterPasswordViewModel> GetChangeMasterPasswordAsync(int userId)
        {
            var model = new ChangeMasterPasswordViewModel
            {
                Sidebar = await GetSidebarDataAsync(userId),
            };
            return model;
        }

        public async Task<DeleteAccountViewModel> GetDeleteAccountPassword(int userId)
        {
            var model = new DeleteAccountViewModel
            {
                Sidebar = await GetSidebarDataAsync(userId),
            };
            return model;
        }

        public async Task<DeleteAccountConfirmationViewModel> GetDeleteAccountConfirmationAsync(int userId)
        {
            var model = new DeleteAccountConfirmationViewModel
            {
                Sidebar = await GetSidebarDataAsync(userId),
            };
            return model;
        }

        public async Task<List<VaultItemViewModel>> GetItemsFromDBAsync(int userId)
        {
            byte[]? encryptionKey = _sessionEncryptionService.GetEncryptionKey(userId);

            if (encryptionKey == null)
            {
                throw new InvalidOperationException("Encryption key doesn't found");
            }

            var items = new List<VaultItemViewModel>();

            // ============ LOGIN ITEMS ============
            var loginItems = await _db.LoginData
                .Where(x => x.UserId == userId)
                .ToListAsync();

            foreach (var item in loginItems)
            {
                try
                {
                    string decryptedLogin = _encryptionService.Decrypt(
                        item.LoginEncrypted!,
                        item.LoginIV!,
                        encryptionKey);

                    string decryptedPassword = _encryptionService.Decrypt(
                        item.PasswordEncrypted!,
                        item.PasswordIV!,
                        encryptionKey);

                    string? decryptedNote = string.IsNullOrEmpty(item.NoteEncrypted)
                        ? null
                        : _encryptionService.Decrypt(
                            item.NoteEncrypted,
                            item.NoteIV!,
                            encryptionKey);

                    items.Add(new LoginItemViewModel
                    {
                        Id = item.Id,
                        FolderId = item.FolderId,
                        Title = item.Title,
                        CreatedAt = item.CreatedAt,
                        WebURL = item.WebURL,
                        Login = decryptedLogin,
                        Password = decryptedPassword,
                        Note = decryptedNote
                    });
                }
                catch (Exception)
                {
                    continue;
                }
            }

            // ============ CARD ITEMS ============
            var cardItems = await _db.CardData
                .Where(x => x.UserId == userId)
                .ToListAsync();

            foreach (var item in cardItems)
            {
                try
                {
                    string decryptedCardNumber = _encryptionService.Decrypt(
                        item.CardNumberEncrypted!,
                        item.CardNumberIV!,
                        encryptionKey);

                    string decryptedExpireMonth = _encryptionService.Decrypt(
                        item.ExpireMonthEncrypted!,
                        item.ExpireMonthIV!,
                        encryptionKey);

                    string decryptedExpireYear = _encryptionService.Decrypt(
                        item.ExpireYearEncrypted!,
                        item.ExpireYearIV!,
                        encryptionKey);

                    string? decryptedNote = string.IsNullOrEmpty(item.NoteEncrypted)
                        ? null
                        : _encryptionService.Decrypt(
                            item.NoteEncrypted,
                            item.NoteIV!,
                            encryptionKey);

                    items.Add(new CardItemViewModel
                    {
                        Id = item.Id,
                        FolderId = item.FolderId,
                        Title = item.Title,
                        CreatedAt = item.CreatedAt,
                        CardNumber = decryptedCardNumber,
                        ExpireMonth = decryptedExpireMonth,
                        ExpireYear = decryptedExpireYear,
                        Note = decryptedNote
                    });
                }
                catch (Exception)
                {
                    continue;
                }
            }

            // ============ NOTE ITEMS ============
            var noteItems = await _db.NoteData
                .Where(x => x.UserId == userId)
                .ToListAsync();

            foreach (var item in noteItems)
            {
                try
                {
                    string decryptedContent = _encryptionService.Decrypt(
                        item.NoteEncrypted!,
                        item.NoteIV!,
                        encryptionKey);

                    items.Add(new NoteItemViewModel
                    {
                        Id = item.Id,
                        FolderId = item.FolderId,
                        Title = item.Title,
                        CreatedAt = item.CreatedAt,
                        Content = decryptedContent
                    });
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return items;
        }
    }
}