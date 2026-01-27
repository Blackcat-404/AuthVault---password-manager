using PasswordManager.Application.Vault;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Infrastructure.Vault
{
    public class AddFolderService : IAddFolderService
    {
        private readonly AppDbContext _db;

        public AddFolderService(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddFolderAsync(FolderDto dto)
        {
            await _db.Folders.AddAsync(new Folder
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Color = dto.Color,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }
    }
}
