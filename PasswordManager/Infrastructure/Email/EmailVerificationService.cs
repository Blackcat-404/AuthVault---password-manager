using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Account.Email;
using PasswordManager.Data;
using PasswordManager.Domain.Enums;

namespace PasswordManager.Infrastructure.Email
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly AppDbContext _db;

        public EmailVerificationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Token == token);
            
            if (user == null)
                return false;

            if (user.Token != token)
                return false;

            if (user.TokenExpiresAt < DateTime.UtcNow)
                return false;

            if (user.EmailVerificationStatus == EmailVerificationStatus.Verified)
                return false;

            return true;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Token == token);

            if (user == null)
                return false;

            user.EmailVerificationStatus = EmailVerificationStatus.Verified;
            user.Token = null;
            user.TokenExpiresAt = null;

            await _db.SaveChangesAsync();    
            return true;
        }
    }
}

