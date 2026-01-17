using Microsoft.EntityFrameworkCore;
using PasswordManager.Application;
using PasswordManager.Application.Account.Login;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Enums;
using PasswordManager.Infrastructure.Security;


namespace PasswordManager.Infrastructure.Login
{
    public class LoginService : ILoginService
    {
        private readonly AppDbContext _db;

        public LoginService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<User>> VerifyLoginAsync(LoginUserDto dto)
        {
            var result = new Result<User>();

            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.Email == dto.Email ||
                u.Login == dto.Email);

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
    }
}
