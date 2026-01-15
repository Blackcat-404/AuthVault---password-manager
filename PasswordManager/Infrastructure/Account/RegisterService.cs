using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Account.Register;
using PasswordManager.Infrastructure.Email;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Enums;
using PasswordManager.Infrastructure.Security;
using PasswordManager.Application;

namespace PasswordManager.Infrastructure.Register
{
    public class RegisterService : IRegisterService
    {
        private readonly AppDbContext _db;
        private readonly EmailService _emailService;

        public RegisterService(AppDbContext db, EmailService emailService)
        {
            _db = db;
            _emailService = emailService;
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
            int verificationCode = VerificationCodeGenerator.Generate();
            DateTime expiresAt = DateTime.UtcNow.AddMinutes(5);

            if (user != null && user.EmailVerificationStatus == EmailVerificationStatus.Verified)
            {
                result.AddError(nameof(dto.Email), "An account with this email already exists");
                return result;
            }

            if (!dto.AcceptTerms)
            {
                result.AddError(nameof(dto.AcceptTerms), "You must accept the terms of use");
                return result;
            }

            if (user == null)
            {
                var userNew = new User
                {
                    Login = dto.Name!,
                    Email = dto.Email!,
                    PasswordHash = PasswordHasher.Hash(dto.Password!),
                    EmailVerificationStatus = EmailVerificationStatus.NotVerified,
                    EmailVerificationCode = verificationCode,
                    EmailVerificationExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    LastLoginAt = null,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Users.Add(userNew);
            }
            else
            {
                user.Login = dto.Name!;
                user.PasswordHash = PasswordHasher.Hash(dto.Password!);
                user.EmailVerificationCode = verificationCode;
                user.EmailVerificationExpiresAt = expiresAt;
            }

            await _db.SaveChangesAsync();

            await SendVerificationEmailAsync(dto.Name, dto.Email, verificationCode, expiresAt);

            return result;
        }

        public async Task SendVerificationEmailAsync(string Name, string Email, int verificationCode, DateTime expiresAt)
        {
            string bodystr = "Hello, " + Name + "\nYour verification code is: " + verificationCode +
                "\n\nThis code expires in 5 minutes\n" +
                "If you did not register, please ignore this email.";

            await _emailService.SendAsync(
                    to: Email!,
                    subject: "Email verification code",
                    body: bodystr
            );
        }
    }
}   
