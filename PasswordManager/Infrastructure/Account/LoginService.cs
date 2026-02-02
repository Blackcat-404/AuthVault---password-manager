using Microsoft.EntityFrameworkCore;
using PasswordManager.Application;
using PasswordManager.Application.Account.Login;
using PasswordManager.Application.Security;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Enums;
using PasswordManager.Infrastructure.Email;
using PasswordManager.Infrastructure.Security;


namespace PasswordManager.Infrastructure.Login
{
    public class LoginService : ILoginService
    {
        private readonly AppDbContext _db;
        private readonly IEncryptionService _encryptionService;
        private readonly ISessionEncryptionService _sessionEncryptionService;
        private readonly EmailService _emailService;
        private readonly TokenService _generateTokenService;

        public LoginService(AppDbContext db, 
            IEncryptionService encryptionService, 
            ISessionEncryptionService sessionEncryptionService, 
            EmailService emailService, 
            TokenService generateTokenService)
        {
            _db = db;
            _emailService = emailService;
            _encryptionService = encryptionService;
            _sessionEncryptionService = sessionEncryptionService;
            _generateTokenService = generateTokenService;
        }

        public Task DeleteEncryptionKey(int userId)
        {
            _sessionEncryptionService.ClearEncryptionKey(userId);
            return Task.CompletedTask;
        }

        public async Task<Result<User>> VerifyLoginAsync(LoginUserDto dto)
        {
            var result = new Result<User>();

            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.Email == dto.Email);

            if (user == null)
            {
                result.AddError(nameof(dto.Email), "Invalid login or password");
                return result;
            }

            if (user.EmailVerificationStatus != EmailVerificationStatus.Verified)
            {
                result.AddError(nameof(dto.Email), "Please verify your email");
                return result;
            }

            bool isPasswordCorrect = _encryptionService.VerifyMasterPassword(
                dto.Password,
                user.AuthHash,
                user.AuthSalt);

            if (!isPasswordCorrect)
            {
                result.AddError(nameof(dto.Email), "Invalid login or password");
                return result;
            }

            byte[] encryptionKey = _encryptionService.DeriveEncryptionKey(
                dto.Password,
                user.EncryptionSalt);

            _sessionEncryptionService.SetEncryptionKey(user.Id, encryptionKey);

            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Result<User>.Ok(user);
        }

        public async Task<bool> Has2FAAsync(string email)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == email);

            if (user == null)
                return false;

            return await _db.TwoFactorAuthentications
                .Where(t => t.UserId == user.Id)
                .Select(t => t.IsEnabled)
                .FirstOrDefaultAsync();
        }
    
        public async Task Send2FACode(int userId)
        {
            int code = VerificationCodeGenerator.Generate();
            var user = await _db.TwoFactorAuthentications
                .FirstOrDefaultAsync(u =>
                    u.UserId == userId
                );

            if (user == null)
            {
                return;
            }

            user.Token = await _generateTokenService.GenerateUniqueResetTokenAsync(_db.Users, t => t.Token);
            user.TokenExpiresAt = DateTime.UtcNow.AddMinutes(5);

            await _db.SaveChangesAsync();
            await _emailService.SendAsync(
                user.Email!,
                "Two-Factor Authentication",
                $"This is your 2FA token:{$"https://localhost/"}\n\nThis code expires in 5 minutes"
            );
        }

        public async Task<bool> Verify2FAToken(int userId, string token)
        {
            var twoFactor = await _db.TwoFactorAuthentications
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (twoFactor == null)
                return false;

            if (twoFactor.TokenExpiresAt < DateTime.UtcNow)
                return false;

            if (twoFactor.Token != token)
                return false;

            twoFactor.Token = null;
            twoFactor.TokenExpiresAt = null;

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
