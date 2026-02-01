namespace PasswordManager.Domain.Entities
{
    public class CardData : BaseEntity
    {
        public string Title { get; set; } = null!;

        public string? CardholderName { get; set; }

        public string? CardNumberEncrypted { get; set; }
        public string? CardNumberIV { get; set; }

        public string? ExpireMonthEncrypted { get; set; }
        public string? ExpireMonthIV { get; set; }

        public string? ExpireYearEncrypted { get; set; }
        public string? ExpireYearIV { get; set; }

        public string? NoteEncrypted { get; set; }
        public string? NoteIV { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? FolderId { get; set; }
        public Folder? Folder { get; set; }
    }
}
