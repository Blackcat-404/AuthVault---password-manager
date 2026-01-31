using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Account.Login
{
    public interface ILoginService
    {
        Task<Result<User>> VerifyLoginAsync(LoginUserDto dto);
        Task<bool> Has2FAAsync(string loginOrEmail);
        Task Send2FACode(int userId);
        Task<bool> Verify2FACode(int userId, string code);
        Task Logout();
    }
}
