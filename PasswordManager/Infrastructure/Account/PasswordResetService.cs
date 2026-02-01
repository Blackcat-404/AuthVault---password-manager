using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Account.ForgotPassword;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;
using PasswordManager.Application;
using PasswordManager.Infrastructure.Email;
using PasswordManager.Infrastructure.Security;

namespace PasswordManager.Infrastructure.ForgotPassword
{
    public class PasswordResetService : IResetPasswordService
    {
        private readonly AppDbContext _db;
        private readonly EmailService _emailService;
        private readonly IEncryptionService _encryptionService;

        public PasswordResetService(AppDbContext db, EmailService emailService, IEncryptionService encryptionService)
        {
            _db = db;
            _emailService = emailService;
            _encryptionService = encryptionService;
        }

        public async Task CreateResetTokenAsync(ForgotPasswordDto dto)
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                {
                    return;
                }

                var token = await GenerateUniqueResetTokenAsync();
                var expiresAt = DateTime.UtcNow.AddMinutes(30);

                var existingToken = await _db.PasswordResetTokens.FirstOrDefaultAsync(t => t.UserId == user.Id);
                if (existingToken != null)
                {
                    if (existingToken.ExpiresAt > DateTime.UtcNow) //Check if 30 minutes passed since last sending
                    {
                        return;
                    }

                    existingToken.Token = token;
                    existingToken.ExpiresAt = expiresAt;
                    existingToken.UsedAt = null;
                }
                else
                {
                    var resetToken = new PasswordResetToken
                    {
                        UserId = user.Id,
                        Token = token,
                        ExpiresAt = expiresAt,
                        UsedAt = null
                    };

                    _db.PasswordResetTokens.Add(resetToken);
                }

                await _db.SaveChangesAsync();

                string bodystr = "Hello, " + user.Login + 
                    "\n\nThis is your link to reset your password:\n" + 
                    "https://localhost:7108/Account/ForgotPassword/ResetPassword?token=" + token +
                    "\n\nWarning! This link can only be used once and resent after 30 minutes for security reasons!" +
                    "\nThe link expires in 30 minutes." +
                    "\nIf it's not you, please ignore this email.";

                await _emailService.SendAsync(
                        to: dto.Email,
                        subject: "Reset password link",
                        body: bodystr
                );
            }

        public async Task<Result> ValidateTokenAsync(string token)
        {
            var result = new Result();

            if (string.IsNullOrWhiteSpace(token))
            {
                result.AddError("","");
                return result;
            }
                
            var resetToken = await _db.PasswordResetTokens.FirstOrDefaultAsync(t =>
                t.Token == token &&
                t.UsedAt == null &&
                t.ExpiresAt > DateTime.UtcNow
            );

            if (resetToken == null)
            {
                result.AddError("","");
                return result;
            }

            return result;
        }

        public async Task<Result> ResetPasswordAsync(string token, string newPassword)
        {
            var result = new Result();

            var resetToken = await _db.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == token &&
                    t.UsedAt == null &&
                    t.ExpiresAt > DateTime.UtcNow
                );

            if (resetToken == null)
            {
                result.AddError("","");
                return result;
            }

            resetToken.Token = null;
            resetToken.UsedAt = DateTime.UtcNow;

            byte[] authSalt = _encryptionService.GenerateSalt();
            byte[] encryptionSalt = _encryptionService.GenerateSalt();
            byte[] authHash = _encryptionService.DeriveAuthHash(newPassword, authSalt);

            resetToken.User.AuthSalt = authSalt;
            resetToken.User.EncryptionSalt = encryptionSalt;
            resetToken.User.AuthHash = authHash;

            await _db.SaveChangesAsync();

            return result;
        }

        private async Task<string> GenerateUniqueResetTokenAsync()
        {
            string token;

            do
            {
                token = Guid.NewGuid().ToString("N");
            }
            while (await _db.PasswordResetTokens.AnyAsync(t => t.Token == token));

            return token;
        }
    }
}
