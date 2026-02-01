using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Infrastructure.Security
{

    public static class PasswordHasher
    {
        private const int KEY_SIZE = 32;
        private const int SALT_SIZE = 32;
        private const int PBKDF2_ITERATIONS = 128000;


        public static byte[] GenerateSalt()
        {
            byte[] salt = new byte[SALT_SIZE];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }


        public static byte[] HashPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password: Encoding.UTF8.GetBytes(password),
                salt: salt,
                iterations: PBKDF2_ITERATIONS,
                hashAlgorithm: HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(KEY_SIZE);
            }
        }

        public static byte[] DeriveEncryptionKey(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password: Encoding.UTF8.GetBytes(password),
                salt: salt,
                iterations: PBKDF2_ITERATIONS,
                hashAlgorithm: HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(KEY_SIZE);
            }
        }

        public static bool VerifyPassword(string password, byte[] storedHash, byte[] salt)
        {
            byte[] computedHash = HashPassword(password, salt);
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
    }
}