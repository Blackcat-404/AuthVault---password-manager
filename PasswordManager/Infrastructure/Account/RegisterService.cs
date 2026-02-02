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
        private readonly TokenService _generateTokenService;

        public RegisterService(AppDbContext db,
                                EmailService emailService,
                                IEncryptionService encryptionService,
                                TokenService generateTokenService)
        {
            _db = db;
            _emailService = emailService;
            _encryptionService = encryptionService;
            _generateTokenService = generateTokenService;
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
                u => u.Login == dto.Name &&
                u.EmailVerificationStatus == EmailVerificationStatus.Verified
            );

            if (loginExists)
            {
                result.AddError(nameof(dto.Name), "This login is already taken");
                return result;
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user != null && user.EmailVerificationStatus == EmailVerificationStatus.Verified)
            {
                result.AddError(nameof(dto.Email), "An account with this email already exists");
                return result;
            }

            string token = await _generateTokenService.GenerateUniqueResetTokenAsync(_db.Users, t => t.Token);
            DateTime expiresAt = DateTime.UtcNow.AddMinutes(5);

            if (user == null)
            {
                byte[] authSalt = _encryptionService.GenerateSalt();
                byte[] encryptionSalt = _encryptionService.GenerateSalt();
                byte[] authHash = _encryptionService.DeriveAuthHash(dto.Password!, authSalt);

                

                var userNew = new User
                {
                    Login = dto.Name!,
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

                user.Login = dto.Name!;
                user.Token = token;
                user.TokenExpiresAt = expiresAt;
            }

            await _db.SaveChangesAsync();

            //Token sending to email
            string bodystr = "Hello, " + dto.Name + "\nYour verification token is: " + $"https://localhost/EmailVerification&token={token}" +
                "\n\nThis token expires in 5 minutes\n" +
                "If you did not register, please ignore this email.";

            await _emailService.SendAsync(
                    to: dto.Email!,
                    subject: "Email verification code",
                    body: bodystr
            );
            return result;
        }

    }
}   
