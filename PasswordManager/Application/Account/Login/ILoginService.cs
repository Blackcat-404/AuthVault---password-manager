using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Account.Login
{
    public interface ILoginService
    {
        Task<Result<User>> VerifyLoginAsync(LoginUserDto dto);
        Task<bool> Has2FAAsync(string loginOrEmail);
        Task Send2FACode(int userId, string email);
        Task<bool> Verify2FACode(int userId, string code);
        Task Logout();
        Task DeleteEncryptionKey(int userId);
    }
}
