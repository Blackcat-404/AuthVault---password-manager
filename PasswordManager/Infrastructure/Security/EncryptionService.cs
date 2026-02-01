using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Infrastructure.Security
{
    /// <summary>
    /// Service for encrypting and decrypting user data
    /// Uses AES-256-GCM for encryption and PBKDF2 for key derivation
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        // AES-256 requires 256 bits = 32 bytes key
        private const int KEY_SIZE = 32;

        // IV (Initialization Vector) size for AES-GCM
        private const int IV_SIZE = 12;

        // Salt size for PBKDF2
        private const int SALT_SIZE = 32;

        // PBKDF2 iterations (higher = more secure but slower)
        // Recommended minimum: 100,000. We use 128,000 for extra security
        private const int PBKDF2_ITERATIONS = 128000;

        // GCM authentication tag size
        private const int TAG_SIZE = 16;

        /// <summary>
        /// Generates a random salt for key derivation
        /// </summary>
        public byte[] GenerateSalt()
        {
            byte[] salt = new byte[SALT_SIZE];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Generates Encryption Key from master password and salt
        /// This key is used to encrypt/decrypt user data
        /// NEVER store this key in database!
        /// </summary>
        public byte[] DeriveEncryptionKey(string masterPassword, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password: Encoding.UTF8.GetBytes(masterPassword),
                salt: salt,
                iterations: PBKDF2_ITERATIONS,
                hashAlgorithm: HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(KEY_SIZE);
            }
        }

        /// <summary>
        /// Generates Authentication Hash from master password and salt
        /// This hash is stored in database for login verification
        /// </summary>
        public byte[] DeriveAuthHash(string masterPassword, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password: Encoding.UTF8.GetBytes(masterPassword),
                salt: salt,
                iterations: PBKDF2_ITERATIONS,
                hashAlgorithm: HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(KEY_SIZE);
            }
        }

        /// <summary>
        /// Encrypts plaintext using AES-256-GCM
        /// Returns: IV + Ciphertext + Tag combined
        /// </summary>
        public EncryptionResult Encrypt(string plaintext, byte[] encryptionKey)
        {
            if (string.IsNullOrEmpty(plaintext))
                return new EncryptionResult { EncryptedData = "", IV = "" };

            // Generate unique IV for this encryption
            byte[] iv = new byte[IV_SIZE];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TAG_SIZE];

            // Encrypt using AES-GCM
            using (var aesGcm = new AesGcm(encryptionKey, TAG_SIZE))
            {
                aesGcm.Encrypt(
                    nonce: iv,
                    plaintext: plaintextBytes,
                    ciphertext: ciphertext,
                    tag: tag);
            }

            // Combine ciphertext + tag
            byte[] encryptedData = new byte[ciphertext.Length + tag.Length];
            Buffer.BlockCopy(ciphertext, 0, encryptedData, 0, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, encryptedData, ciphertext.Length, tag.Length);

            return new EncryptionResult
            {
                EncryptedData = Convert.ToBase64String(encryptedData),
                IV = Convert.ToBase64String(iv)
            };
        }

        /// <summary>
        /// Decrypts ciphertext using AES-256-GCM
        /// </summary>
        public string Decrypt(string encryptedDataBase64, string ivBase64, byte[] encryptionKey)
        {
            if (string.IsNullOrEmpty(encryptedDataBase64) || string.IsNullOrEmpty(ivBase64))
                return "";

            try
            {
                byte[] iv = Convert.FromBase64String(ivBase64);
                byte[] encryptedData = Convert.FromBase64String(encryptedDataBase64);

                // Extract ciphertext and tag
                int ciphertextLength = encryptedData.Length - TAG_SIZE;
                byte[] ciphertext = new byte[ciphertextLength];
                byte[] tag = new byte[TAG_SIZE];

                Buffer.BlockCopy(encryptedData, 0, ciphertext, 0, ciphertextLength);
                Buffer.BlockCopy(encryptedData, ciphertextLength, tag, 0, TAG_SIZE);

                // Decrypt
                byte[] plaintextBytes = new byte[ciphertext.Length];

                using (var aesGcm = new AesGcm(encryptionKey, TAG_SIZE))
                {
                    aesGcm.Decrypt(
                        nonce: iv,
                        ciphertext: ciphertext,
                        tag: tag,
                        plaintext: plaintextBytes);
                }

                return Encoding.UTF8.GetString(plaintextBytes);
            }
            catch (CryptographicException)
            {
                // Decryption failed - wrong key or corrupted data
                throw new InvalidOperationException("Decryption failed. Invalid encryption key or corrupted data.");
            }
        }

        /// <summary>
        /// Verifies if master password is correct by comparing auth hashes
        /// </summary>
        public bool VerifyMasterPassword(string masterPassword, byte[] storedAuthHash, byte[] authSalt)
        {
            byte[] computedAuthHash = DeriveAuthHash(masterPassword, authSalt);
            return CryptographicOperations.FixedTimeEquals(computedAuthHash, storedAuthHash);
        }
    }

    /// <summary>
    /// Result of encryption operation
    /// </summary>
    public class EncryptionResult
    {
        /// <summary>
        /// Base64-encoded encrypted data (ciphertext + tag)
        /// </summary>
        public string EncryptedData { get; set; } = "";

        /// <summary>
        /// Base64-encoded Initialization Vector
        /// Must be unique for each encryption
        /// </summary>
        public string IV { get; set; } = "";
    }

    /// <summary>
    /// Interface for encryption service
    /// </summary>
    public interface IEncryptionService
    {
        byte[] GenerateSalt();
        byte[] DeriveEncryptionKey(string masterPassword, byte[] salt);
        byte[] DeriveAuthHash(string masterPassword, byte[] salt);
        EncryptionResult Encrypt(string plaintext, byte[] encryptionKey);
        string Decrypt(string encryptedDataBase64, string ivBase64, byte[] encryptionKey);
        bool VerifyMasterPassword(string masterPassword, byte[] storedAuthHash, byte[] authSalt);
    }
}