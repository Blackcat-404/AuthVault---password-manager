namespace PasswordManager.Domain.Entities
{
    public class LoginData : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string? LoginEncrypted { get; set; }

        public string? PasswordEncrypted { get; set; }
        public string? NoteEncrypted { get; set; }

        public string? WebURL { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? FolderId { get; set; }
        public Folder? Folder { get; set; }
    }
}
