namespace PasswordManager.ViewModels.Vault.VaultItems
{
    public class LoginItemViewModel : VaultItemViewModel
    {
        public string? Login { get; set; }
        public string? Password { get; set; }
        public List<string>? Note { get; set; }
        public string? WebURL { get; set; }
    }
}