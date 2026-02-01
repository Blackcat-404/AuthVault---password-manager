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

        public LoginService(AppDbContext db, IEncryptionService encryptionService, ISessionEncryptionService sessionEncryptionService)
        {
            _db = db;
            _encryptionService = encryptionService;
            _sessionEncryptionService = sessionEncryptionService;
        }

        public Task DeleteEncryptionKey(int userId)
        {
            _sessionEncryptionService.ClearEncryptionKey(userId);
            return Task.CompletedTask;
        }

        public async Task<Result<User>> VerifyLoginAsync(LoginUserDto dto)
        {
            var result = new Result<User>();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

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
    }
}
