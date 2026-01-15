namespace PasswordManager.Application.Account.Register
{
    public interface IRegisterService
    {
        Task<Result> RegisterUserAsync(RegisterUserDto dto);
    }
}
