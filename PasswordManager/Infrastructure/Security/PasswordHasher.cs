namespace PasswordManager.Infrastructure.Security;

using System.Security.Cryptography;
using System.Text;

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        using var sha256 = SHA256.Create();
        return Convert.ToBase64String(
            sha256.ComputeHash(Encoding.UTF8.GetBytes(password))
        );
    }

    public static bool Verify(string password, string passwordHash2)
    {
        return Hash(password) == passwordHash2;
    }
}