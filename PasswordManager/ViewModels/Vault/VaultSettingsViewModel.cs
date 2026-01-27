namespace PasswordManager.ViewModels.Vault
{
    public class VaultSettingsViewModel
    {
        public VaultSidebarViewModel Sidebar { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FAEmail { get; set; }
        public bool Is2FAEnabled { get; set; }
        public string accountCreatedOn { get; set; } = null!;
    }
}
