using Microsoft.EntityFrameworkCore;
using PasswordManager.Data;
using PasswordManager.Domain.Enums;
using PasswordManager.Infrastructure.Security;

namespace PasswordManager.Infrastructure.Email
{
    public class EmailVerificationService
    {
        private readonly AppDbContext _db;
        private readonly EmailService _emailService;

        public EmailVerificationService(AppDbContext db, EmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public async Task VerifyAsync(string email, int code)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (user.EmailVerificationExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("Verification code expired");

            if (user.EmailVerificationCode != code)
                throw new InvalidOperationException("Invalid verification code");

            user.EmailVerificationStatus = EmailVerificationStatus.Verified;
            user.EmailVerificationCode = null;
            user.EmailVerificationExpiresAt = null;

            await _db.SaveChangesAsync();
        }

        public async Task ResendAsync(string email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (user.EmailVerificationExpiresAt > DateTime.UtcNow)
                throw new InvalidOperationException("The code can still be used");

            if (user.EmailVerificationStatus == EmailVerificationStatus.Verified)
                throw new InvalidOperationException("Email is already confirmed");

            var code = VerificationCodeGenerator.Generate();

            user.EmailVerificationCode = code;
            user.EmailVerificationExpiresAt = DateTime.UtcNow.AddMinutes(5);

            await _db.SaveChangesAsync();

            string bodystr = "Hello, " + user.Login + "\nYour verification code is: " + code +
                "\n\nThis code expires in 5 minutes\n" +
                "If you did not register, please ignore this email.";

            await _emailService.SendAsync(
                user.Email,
                "Resending email verification code",
                bodystr
            );
        }
    }
}

