using Microsoft.EntityFrameworkCore;
using PasswordManager.Application;
using PasswordManager.Application.Account.Register;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Enums;
using PasswordManager.Infrastructure.Email;
using PasswordManager.Infrastructure.Security;

namespace PasswordManager.Infrastructure.Register
{
    public class RegisterService : IRegisterService
    {
        private readonly AppDbContext _db;
        private readonly EmailService _emailService;
        private readonly IEncryptionService _encryptionService;
        private readonly TokenService _tokenService;

        public RegisterService(AppDbContext db,
                                EmailService emailService,
                                IEncryptionService encryptionService,
                                TokenService tokenService)
        {
            _db = db;
            _emailService = emailService;
            _encryptionService = encryptionService;
            _tokenService = tokenService;
        }


        public async Task<Result> RegisterUserAsync(RegisterUserDto dto)
        {
            var result = new Result();

            //Condition where the mail that sends the code hasn't been entered. Change example@gmail.com
            /*if (dto.Email == "example@gmail.com")
            {
                result.AddError(nameof(dto.Name), "Are you serious, dude?");
                return result;
            }*/
            
            var loginExists = await _db.Users.AnyAsync(
                u => u.Login == dto.Login &&
                u.EmailVerificationStatus == EmailVerificationStatus.Verified
            );

            if (loginExists)
            {
                result.AddError(nameof(dto.Login), "This login is already taken");
                return result;
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user != null && user.EmailVerificationStatus == EmailVerificationStatus.Verified)
            {
                result.AddError(nameof(dto.Email), "An account with this email already exists");
                return result;
            }

            string token = await _tokenService.GenerateUniqueResetTokenAsync(_db.Users, t => t.Token);
            DateTime expiresAt = DateTime.UtcNow.AddMinutes(5);

            if (user == null)
            {
                byte[] authSalt = _encryptionService.GenerateSalt();
                byte[] encryptionSalt = _encryptionService.GenerateSalt();
                byte[] authHash = _encryptionService.DeriveAuthHash(dto.Password!, authSalt);

                

                var userNew = new User
                {
                    Login = dto.Login!,
                    Email = dto.Email!,

                    AuthHash = authHash,
                    AuthSalt = authSalt,
                    EncryptionSalt = encryptionSalt,

                    EmailVerificationStatus = EmailVerificationStatus.NotVerified,
                    Token = token,
                    TokenExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    LastLoginAt = null,
                    CreatedAt = DateTime.UtcNow
                };

                await _db.Users.AddAsync(userNew);
            }
            else
            {
                byte[] authSalt = _encryptionService.GenerateSalt();
                byte[] encryptionSalt = _encryptionService.GenerateSalt();
                byte[] authHash = _encryptionService.DeriveAuthHash(dto.Password, authSalt);

                user.AuthHash = authHash;
                user.AuthSalt = authSalt;
                user.EncryptionSalt = encryptionSalt;

                user.Login = dto.Login!;
                user.Token = token;
                user.TokenExpiresAt = expiresAt;
            }

            await _db.SaveChangesAsync();
            await _tokenService.SendTokenToEmailAsync(dto.Login, dto.Email, 5, $"https://localhost:7108/Account/Register/VerifyEmail?token={token}");

            return result;
        }

    }
}   
