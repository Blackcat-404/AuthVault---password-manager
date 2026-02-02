using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Account.ForgotPassword;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;
using PasswordManager.Infrastructure.Email;
using PasswordManager.Infrastructure.Security;

namespace PasswordManager.Infrastructure.ForgotPassword
{
    public class PasswordResetService : IResetPasswordService
    {
        private readonly AppDbContext _db;
        private readonly EmailService _emailService;
        private readonly IEncryptionService _encryptionService;
        private readonly TokenService _tokenService;

        public PasswordResetService(AppDbContext db, EmailService emailService, IEncryptionService encryptionService,TokenService tokenService)
        {
            _db = db;
            _emailService = emailService;
            _encryptionService = encryptionService;
            _tokenService = tokenService;
        }

        public async Task CreateResetTokenAsync(ForgotPasswordDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                return;
            }

            var token = await _tokenService.GenerateUniqueResetTokenAsync(_db.PasswordResetTokens, t => t.Token);
            var expiresAt = DateTime.UtcNow.AddMinutes(30);

            var existingToken = await _db.PasswordResetTokens.FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (existingToken != null)
            {

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

            await _tokenService.SendTokenToEmailAsync(user.Login, dto.Email, 30, $"https://localhost:7108/Account/ForgotPassword/ResetPassword?token={token}");
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }
                
            var resetToken = await _db.PasswordResetTokens.FirstOrDefaultAsync(t =>
                t.Token == token &&
                t.UsedAt == null &&
                t.ExpiresAt > DateTime.UtcNow
            );

            if (resetToken == null)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var resetToken = await _db.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == token &&
                    t.UsedAt == null &&
                    t.ExpiresAt > DateTime.UtcNow
                );

            if (resetToken == null)
                return false;

            resetToken.Token = null;
            resetToken.UsedAt = DateTime.UtcNow;

            byte[] authSalt = _encryptionService.GenerateSalt();
            byte[] encryptionSalt = _encryptionService.GenerateSalt();
            byte[] authHash = _encryptionService.DeriveAuthHash(newPassword, authSalt);

            resetToken.User.AuthSalt = authSalt;
            resetToken.User.EncryptionSalt = encryptionSalt;
            resetToken.User.AuthHash = authHash;

            await _db.SaveChangesAsync();

            return true;
        }
    }
}
