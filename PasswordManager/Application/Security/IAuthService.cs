namespace PasswordManager.Application.Security
{
    public interface IAuthService
    {
        Task SignInAsync(HttpContext context, int userId);
        Task SignOutAsync(HttpContext context);
    }

}
