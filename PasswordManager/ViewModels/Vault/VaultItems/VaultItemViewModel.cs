namespace PasswordManager.ViewModels.Vault.VaultItems
{
    public class VaultItemViewModel
    {
        public int Id { get; set; }
        public int? FolderId { get; set; }
        public string? Title { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}