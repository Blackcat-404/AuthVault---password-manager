namespace PasswordManager.ViewModels.Vault
{
    public class VaultSettingsViewModel
    {
        public VaultSidebarViewModel Sidebar { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string accountCreatedOn { get; set; } = null!;
    }
}
