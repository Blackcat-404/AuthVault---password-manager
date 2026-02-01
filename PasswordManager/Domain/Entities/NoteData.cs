namespace PasswordManager.Domain.Entities
{
    public class NoteData : BaseEntity
    {
        public string Title { get; set; } = null!;

        public string? NoteEncrypted { get; set; }
        public string? NoteIV { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? FolderId { get; set; }
        public Folder? Folder { get; set; }
    }
}
