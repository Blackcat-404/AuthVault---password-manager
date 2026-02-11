using Microsoft.EntityFrameworkCore;
using PasswordManager.Application;
using PasswordManager.Application.Account.Login;
using PasswordManager.Application.Security;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Enums;
using PasswordManager.Infrastructure.Security;


namespace PasswordManager.Infrastructure.Login
{
    public class LoginService : ILoginService
    {
        private readonly AppDbContext _db;
        private readonly IEncryptionService _encryptionService;
        private readonly ISessionEncryptionService _sessionEncryptionService;
        private readonly TokenService _tokenService;

        public LoginService(AppDbContext db, 
            IEncryptionService encryptionService, 
            ISessionEncryptionService sessionEncryptionService, 
            TokenService tokenService)
        {
            _db = db;
            _encryptionService = encryptionService;
            _sessionEncryptionService = sessionEncryptionService;
            _tokenService = tokenService;
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
    
        public async Task Send2FACode(int userId,string baseURL)
        {
            var user = await _db.TwoFactorAuthentications
                .FirstOrDefaultAsync(u =>
                    u.UserId == userId
                );

            if (user == null)
            {
                return;
            }

            var token = await _tokenService.GenerateUniqueResetTokenAsync(_db.Users, t => t.Token);
            user.Token = token;
            user.TokenExpiresAt = DateTime.UtcNow.AddMinutes(5);

            await _db.SaveChangesAsync();
            await _tokenService.SendTokenToEmailAsync(user.User.Login, user.Email!, 5, $"{baseURL}/Account/Login/2FA?token={token}");
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

        public async Task<int?> GetUserIdByToken(string token)
        {
            var twoFactor = await _db.TwoFactorAuthentications
                .FirstOrDefaultAsync(x => x.Token == token && x.TokenExpiresAt > DateTime.UtcNow);

            return twoFactor?.UserId;
        }

        public async Task<bool> IsTokenVerified(int userId)
        {
            var twoFactor = await _db.TwoFactorAuthentications
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (twoFactor == null)
                return false;

            return twoFactor.Token == null;
        }
    }
}
