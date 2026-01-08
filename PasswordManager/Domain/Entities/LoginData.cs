namespace PasswordManager.Domain.Entities
{
    public class LoginData : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Login { get; set; } = null!;

        public string PasswordEncrypted { get; set; } = null!;
        public string NoteEncrypted { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int FolderId { get; set; }
        public Folder Folder { get; set; } = null!;
    }
}
