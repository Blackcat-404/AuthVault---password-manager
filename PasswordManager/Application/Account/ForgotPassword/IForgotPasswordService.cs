namespace PasswordManager.Application.Account.ForgotPassword
{
    public interface IResetPasswordService
    {
        Task CreateResetTokenAsync(ForgotPasswordDto dto);
        Task<Result> ValidateTokenAsync(string token);
        Task<Result> ResetPasswordAsync(string token, string newPassword);
    }
}