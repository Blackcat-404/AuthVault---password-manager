using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Account.Email
{
    public interface IEmailVerificationService
    {
        Task<bool> VerifyTokenAsync(string token);
        Task<bool> VerifyEmailAsync(string token);
    }
}
