namespace PasswordManager.ViewModels.Vault
{
    public class AddItemViewModel
    {
        public VaultSidebarViewModel Sidebar { get; set; } = null!;
        public string ItemType { get; set; } = "login";
    }
}
