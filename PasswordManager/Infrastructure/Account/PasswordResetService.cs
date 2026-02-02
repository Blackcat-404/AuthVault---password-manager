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
        private readonly TokenService _generateTokenService;

        public PasswordResetService(AppDbContext db, EmailService emailService, IEncryptionService encryptionService,TokenService generateTokenService)
        {
            _db = db;
            _emailService = emailService;
            _encryptionService = encryptionService;
            _generateTokenService = generateTokenService;
        }

        public async Task CreateResetTokenAsync(ForgotPasswordDto dto)
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                {
                    return;
                }

                var token = await _generateTokenService.GenerateUniqueResetTokenAsync(_db.PasswordResetTokens, t => t.Token);
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
    }
}
