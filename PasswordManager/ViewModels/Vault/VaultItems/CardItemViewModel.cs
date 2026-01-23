namespace PasswordManager.ViewModels.Vault.VaultItems
{
    public class CardItemViewModel : VaultItemViewModel
    {
        public string? CardNumber { get; set; }
        public string? ExpireMonth { get; set; }
        public string? ExpireYear { get; set; }
        public string? CardholderName { get; set; }
        public string? Note { get; set; }
    }
}