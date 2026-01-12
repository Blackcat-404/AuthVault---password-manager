using Microsoft.EntityFrameworkCore;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Enums;
using PasswordManager.Infrastructure.Security;

namespace PasswordManager.Infrastructure.Login
{
    public class LoginService
    {
        private readonly AppDbContext _db;

        public LoginService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<User> VerifyLoginAsync(string loginOrEmail, string password)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == loginOrEmail ||
                    u.Login == loginOrEmail);

            if (user == null)
                throw new InvalidOperationException("Invalid login or password");

            if (user.EmailVerificationStatus != EmailVerificationStatus.Verified)
                throw new InvalidOperationException("Please verify your email");

            if (!PasswordHasher.Verify(password, user.PasswordHash))
                throw new InvalidOperationException("Invalid login or password");

            return user;
        }
    }
}
