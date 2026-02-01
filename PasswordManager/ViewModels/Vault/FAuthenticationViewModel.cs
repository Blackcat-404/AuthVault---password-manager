namespace PasswordManager.ViewModels.Vault
{
    public class FAuthenticationEmailViewModel
    {
        public VaultSidebarViewModel Sidebar { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class FAuthenticationCodeViewModel
    {
        public VaultSidebarViewModel Sidebar { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}
