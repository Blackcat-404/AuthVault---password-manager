using Microsoft.EntityFrameworkCore;
using PasswordManager.Application;
using PasswordManager.Application.Account.Login;
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
        private readonly EmailService _emailService;

        public LoginService(AppDbContext db,EmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public Task Logout()
        {
            throw new NotImplementedException();
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

            if (!PasswordHasher.Verify(dto.Password, user.PasswordHash))
            {
                result.AddError(nameof(dto.Email), "Invalid login or password");
                return result;
            }

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
    
        public async Task Send2FACode(int userId,string email)
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

            user.Code = code.ToString();
            user.ExpiresAt = DateTime.UtcNow.AddMinutes(5);

            await _db.SaveChangesAsync();
            await _emailService.SendAsync(
                email,
                "Two-Factor Authentication",
                $"This is your 2FA code:{code}\n\nThis code expires in 5 minutes"
            );
        }

        public async Task<bool> Verify2FACode(int userId, string code)
        {
            var twoFactor = await _db.TwoFactorAuthentications
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (twoFactor == null)
                return false;

            if (twoFactor.ExpiresAt < DateTime.UtcNow)
                return false;

            if (twoFactor.Code != code)
                return false;

            twoFactor.Code = null;
            twoFactor.ExpiresAt = null;

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
