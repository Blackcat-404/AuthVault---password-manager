using PasswordManager.ViewModels.Vault.VaultItems;

namespace PasswordManager.ViewModels.Vault
{
    public class VaultHomeViewModel
    {
        public VaultSidebarViewModel Sidebar { get; set; } = null!;
        public IReadOnlyList<VaultItemViewModel> Items { get; init; } = Array.Empty<VaultItemViewModel>();
        public int? CurrentFolderId { get; set; }
        public string? CurrentFolderName { get; set; }
        public string? CurrentFolderDescription { get; set; }
    }
}
