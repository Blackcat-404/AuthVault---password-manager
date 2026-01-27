using PasswordManager.Application.Vault;
using PasswordManager.ViewModels.Vault;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;
using PasswordManager.ViewModels.Vault.VaultItems;

namespace PasswordManager.Infrastructure.Vault
{
    public class GetFolderService : IGetFolderService, IGetAllFoldersService
    {
        private readonly AppDbContext _db;
        private readonly IVaultHomeService _vaultHomeService;
        private readonly IVaultSidebarService _vaultSidebarService;

        public GetFolderService(AppDbContext db, IVaultHomeService vaultHomeService, IVaultSidebarService vaultSidebarService)
        {
            _db = db;
            _vaultHomeService = vaultHomeService;
            _vaultSidebarService = vaultSidebarService;
        }

        public async Task<VaultHomeViewModel> GetFolderAsync(int userId, int folderId)
        {
            var folder = await _db.Folders
                .Where(f => f.Id == folderId && f.UserId == userId)
                .FirstOrDefaultAsync();

            var allItems = await _vaultHomeService.GetItemsFromDBAsync(userId);

            var folderItems = allItems
                .Where(i => i.FolderId == folderId)
                .ToList();

            var model = new VaultHomeViewModel
            {
                Sidebar = await _vaultSidebarService.GetSidebarDataAsync(userId),
                Items = folderItems,
                CurrentFolderId = folderId,
                CurrentFolderName = folder?.Name,
                CurrentFolderDescription = folder?.Description
            };
            return model;
        }

        public async Task<Dictionary<int, string>> GetAllFoldersAsync(int userId)
        {
            var folders = await _db.Folders
                .Where(u => u.UserId == userId)
                .ToDictionaryAsync(f => f.Id, f => f.Name);

            return folders;
        }
    }
}
