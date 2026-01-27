using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Vault
{
    public interface IGetAllFoldersService
    {
        Task<Dictionary<int, string>> GetAllFoldersAsync(int userId);
    }
}
