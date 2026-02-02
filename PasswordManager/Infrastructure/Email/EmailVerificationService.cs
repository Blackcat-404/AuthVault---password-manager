using Microsoft.EntityFrameworkCore;
using PasswordManager.Application;
using PasswordManager.Application.Account.Email;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Enums;
using PasswordManager.Infrastructure.Security;

namespace PasswordManager.Infrastructure.Email
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly AppDbContext _db;
        private readonly EmailService _emailService;
        private readonly TokenService _tokenService;

        public EmailVerificationService(AppDbContext db, EmailService emailService, TokenService tokenService)
        {
            _db = db;
            _emailService = emailService;
            _tokenService = tokenService;
        }

        public async Task<Result<User>> VerifyAsync(EmailVerificationDto dto)
        {
            var result = new Result<User>();

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                result.AddError(nameof(dto.Email), "User not found");
                return result;
            }

            if (user.TokenExpiresAt < DateTime.UtcNow)
            {
                result.AddError(nameof(dto.Token), "Verification code expired");
                return result;
            }

            if (user.Token != dto.Token)
            {
                result.AddError(nameof(dto.Token), "Invalid token");
                return result;
            }

            user.EmailVerificationStatus = EmailVerificationStatus.Verified;
            user.Token = null;
            user.TokenExpiresAt = null;

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

            if (user.TokenExpiresAt > DateTime.UtcNow.AddMinutes(4))
            {
                result.AddError(nameof(dto.Token), "The code can still be used");
                return result;
            }

            if (user.EmailVerificationStatus == EmailVerificationStatus.Verified)
            {
                result.AddError(nameof(Email), "Email is already confirmed");
                return result;
            }

            var token = await _tokenService.GenerateUniqueResetTokenAsync(_db.Users, t => t.Token);

            user.Token = token;
            user.TokenExpiresAt = DateTime.UtcNow.AddMinutes(5);

            await _db.SaveChangesAsync();
            //await _tokenService.SendTokenToEmailAsync(user.Login, dto.Email, "5", token);

            return result;
        }
    }
}

