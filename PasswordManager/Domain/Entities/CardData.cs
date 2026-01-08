namespace PasswordManager.Domain.Entities
{
    public class CardData : BaseEntity
    {
        public string Title { get; set; } = null!;

        public string CardNumberEncrypted { get; set; } = null!;
        public string ExpireMonthEncrypted { get; set; } = null!;
        public string ExpireYearEncrypted { get; set; } = null!;
        public string NoteEncrypted { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int FolderId { get; set; }
        public Folder Folder { get; set; } = null!;
    }
}
