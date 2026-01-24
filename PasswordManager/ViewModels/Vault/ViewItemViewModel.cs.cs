using PasswordManager.ViewModels.Vault.VaultItems;

namespace PasswordManager.ViewModels.Vault
{
    public class ViewItemViewModel
    {
        public VaultSidebarViewModel Sidebar { get; set; } = null!;
        public VaultItemViewModel Item { get; set; } = null!;
    }
}