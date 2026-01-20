using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Account.Login
{
    public interface ILoginService
    {
        Task<Result<User>> VerifyLoginAsync(LoginUserDto dto);
        Task Logout();
    }
}
