using Microsoft.EntityFrameworkCore;
using PasswordManager.Infrastructure.Email;
using System.Linq.Expressions;

namespace PasswordManager.Infrastructure.Security
{
    public class TokenService
    {
        private readonly EmailService _emailService;

        public TokenService(EmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<string> GenerateUniqueResetTokenAsync<T>(
            DbSet<T> dbSet,
            Expression<Func<T, string>> tokenSelector)
            where T : class
        {
            string token;

            do
            {
                token = Guid.NewGuid().ToString("N");
            }
            while (await dbSet.AnyAsync(entity =>
                EF.Property<string>(entity, ((MemberExpression)tokenSelector.Body).Member.Name) == token
            ));

            return token;
        }

        public async Task SendTokenToEmailAsync(string login, string email , int expireDuration, string link)
        {
            string bodystr = "Hello, " + login + "\nYour verification link is: " + link +
                "\n\nThis link expires in " + expireDuration + " minutes\n" +
                "If you did not register, please ignore this email.";

            await _emailService.SendAsync(
                email,
                "Link",
                bodystr
            );
        }
    }
}
