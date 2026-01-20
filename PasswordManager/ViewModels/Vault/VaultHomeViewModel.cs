using PasswordManager.ViewModels.Vault.VaultItems;

namespace PasswordManager.ViewModels.Vault
{
    public class VaultHomeViewModel
    {
        public VaultSidebarViewModel Sidebar { get; set; } = null!;
        public IReadOnlyList<VaultItemViewModel> Items { get; init; } = Array.Empty<VaultItemViewModel>();
    }
}
