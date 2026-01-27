using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Vault;
using PasswordManager.Data;

namespace PasswordManager.Infrastructure.Vault
{
    public class DeleteFolderService : IDeleteFolderService
    {
        private readonly AppDbContext _db;

        public DeleteFolderService(AppDbContext db)
        {
            _db = db;
        }

        public async Task DeleteFolderAsync(int userId, int folderId)
        {
            // Verify folder belongs to user
            var folder = await _db.Folders
                .Where(f => f.Id == folderId && f.UserId == userId)
                .FirstOrDefaultAsync();

            // Move all items in this folder to "No Folder" (set FolderId to null)
            var loginItems = await _db.LoginData
                .Where(l => l.FolderId == folderId && l.UserId == userId)
                .ToListAsync();

            if (folder == null)
            {
                throw new InvalidOperationException("Folder not found or access denied");
            }

            var cardItems = await _db.CardData
                .Where(c => c.FolderId == folderId && c.UserId == userId)
                .ToListAsync();

            foreach (var item in cardItems)
            {
                item.FolderId = null;
            }

            var noteItems = await _db.NoteData
                .Where(n => n.FolderId == folderId && n.UserId == userId)
                .ToListAsync();

            foreach (var item in noteItems)
            {
                item.FolderId = null;
            }

            // Delete the folder
            _db.Folders.Remove(folder!);

            // Save changes
            await _db.SaveChangesAsync();
        }
    }
}
