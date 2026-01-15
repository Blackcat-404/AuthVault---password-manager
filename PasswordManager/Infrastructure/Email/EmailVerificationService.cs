using Microsoft.EntityFrameworkCore;
using PasswordManager.Data;
using PasswordManager.Domain.Enums;
using PasswordManager.Domain.Entities;
using PasswordManager.Infrastructure.Security;
using PasswordManager.Application;
using PasswordManager.Application.Account.Email;

namespace PasswordManager.Infrastructure.Email
{
    public class EmailVerificationService
    {
        private readonly AppDbContext _db;
        private readonly EmailService _emailService;

        public EmailVerificationService(AppDbContext db, EmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public async Task<Result<User>> VerifyAsync(EmailVerificationDto dto)
        {
            var result = new Result<User>();

            var user = await _db.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == dto.Email ||
                    u.Login == dto.Email);

            if (user == null)
            {
                result.AddError(nameof(dto.Email), "User not found");
                return result;
            }

            if (user.EmailVerificationExpiresAt < DateTime.UtcNow)
            {
                result.AddError(nameof(dto.VerificationCode), "Verification code expired");
                return result;
            }

            if (user.EmailVerificationCode != dto.VerificationCode)
            {
                result.AddError(nameof(dto.VerificationCode), "Invalid verification code");
                return result;
            }

            user.EmailVerificationStatus = EmailVerificationStatus.Verified;
            user.EmailVerificationCode = null;
            user.EmailVerificationExpiresAt = null;

            user!.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Result<User>.Ok(user);
        }

        public async Task<Result> ResendAsync(EmailVerificationDto dto)
        {
            var result = new Result();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                result.AddError(nameof(Email), "User not found");
                return result;
            }

            if (user.EmailVerificationExpiresAt > DateTime.UtcNow.AddMinutes(4))
            {
                result.AddError(nameof(dto.VerificationCode), "The code can still be used");
                return result;
            }

            if (user.EmailVerificationStatus == EmailVerificationStatus.Verified)
            {
                result.AddError(nameof(Email), "Email is already confirmed");
                return result;
            }

            var code = VerificationCodeGenerator.Generate();

            user.EmailVerificationCode = code;
            user.EmailVerificationExpiresAt = DateTime.UtcNow.AddMinutes(5);

            await _db.SaveChangesAsync();

            string bodystr = "Hello, " + user.Login + "\nYour verification code is: " + code +
                "\n\nThis code expires in 5 minutes\n" +
                "If you did not register, please ignore this email.";

            await _emailService.SendAsync(
                user.Email,
                "Resending email verification code",
                bodystr
            );

            return result;
        }
    }
}

