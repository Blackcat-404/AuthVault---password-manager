namespace PasswordManager.Application.Account.ForgotPassword
{
    public interface IResetPasswordService
    {
        Task CreateResetTokenAsync(ForgotPasswordDto dto);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}