using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PasswordManager.Domain.Enums;

namespace PasswordManager.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Login { get; set; } = null!;
        public string Email { get; set; } = null!;

        public byte[] AuthHash { get; set; } = null!;
        public byte[] AuthSalt { get; set; } = null!;
        public byte[] EncryptionSalt { get; set; } = null!;


        public EmailVerificationStatus EmailVerificationStatus { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenExpiresAt { get; set; }

        public DateTime? LastLoginAt { get; set; }
        public DateTime? PasswordLastChangedAt { get; set; } = null;

        public int SessionTimeoutMinutes { get; set; } = 15;

        public ICollection<Folder> Folders { get; set; } = new List<Folder>();
    }
}
