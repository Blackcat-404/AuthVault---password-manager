using System.Security.Cryptography;

namespace PasswordManager.Infrastructure.Security
{
    public static class VerificationCodeGenerator
    {
        public static int Generate()
        {
            return RandomNumberGenerator.GetInt32(100000, 999999);
        }
    }
}