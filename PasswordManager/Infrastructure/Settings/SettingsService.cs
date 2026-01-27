using Microsoft.EntityFrameworkCore;
using PasswordManager.Data;
using PasswordManager.Infrastructure.Email;
using PasswordManager.Infrastructure.Security;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Infrastructure.Settings
{
   
    public class SettingsService 
    {
        private readonly AppDbContext _db;
        private readonly EmailService _emailService;
        public SettingsService(AppDbContext db,EmailService emailService) 
        {
            _db = db;
            _emailService = emailService;
        }

        public async Task Add2FAAsync(int userId,string email)
        {
            int code = VerificationCodeGenerator.Generate();
            var expiresAt = DateTime.UtcNow.AddMinutes(5);

            var twoFa = await _db.TwoFactorAuthentications
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (twoFa == null)
            {
                twoFa = new TwoFactorAuthentication
                {
                    UserId = userId,
                    Code = code.ToString(),
                    ExpiresAt = expiresAt
                };

                _db.TwoFactorAuthentications.Add(twoFa);
            }
            else
            {
                twoFa.Code = code.ToString();
                twoFa.ExpiresAt = expiresAt;
            }

            await _db.SaveChangesAsync();
            await _emailService.SendAsync(
                email,
                "Your verification code",
                $"Your verification code for 2FA is: {code}\n\nIt expires in 5 minutes."
            );
        }

        public async Task<bool> Verify2FACodeAsync(int userId, string code, string email)
        {
            var record = await _db.TwoFactorAuthentications
            .Where(x =>
                x.UserId == userId &&
                !x.IsEnabled)
            .FirstOrDefaultAsync();

            if (record == null)
            {
                return false;
            }

            if (record.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            if (record.Code != code)
            {
                return false;
            }

            record.Email = email;
            record.Code = null;
            record.ExpiresAt = null;
            record.LinkedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
