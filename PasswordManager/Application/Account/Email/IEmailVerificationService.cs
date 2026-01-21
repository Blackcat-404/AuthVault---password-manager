using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Account.Email
{
    public interface IEmailVerificationService
    {
        Task<Result<User>> VerifyAsync(EmailVerificationDto dto);
        Task<Result> ResendAsync(EmailVerificationDto dto);
    }
}
