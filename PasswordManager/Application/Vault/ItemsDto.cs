namespace PasswordManager.Application.Vault
{
    public class VaultItemDto
    {
        public int Id { get; set; }
        public int? FolderId { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LoginItemDto : VaultItemDto
    {
        public string Login { get; set; } = null!;
        public string? Password { get; set; }
        public string? Note { get; set; }
        public string? WebURL { get; set; }
    }

    public class CardItemDto : VaultItemDto
    {
        public string? CardNumber { get; set; }
        public string? CardholderName { get; set; }
        public string? ExpireMonth { get; set; }
        public string? ExpireYear { get; set; }
        public string? Note { get; set; }
    }

    public class NoteItemDto : VaultItemDto
    {
        public string? Note { get; set; }
    }
}
