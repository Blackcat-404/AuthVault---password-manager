using PasswordManager.ViewModels.Vault.VaultItems;

namespace PasswordManager.ViewModels.Vault
{
    public class AddItemViewModel
    {
        public string ItemType { get; set; } = "login";
        public string Title { get; set; } = null!;
        public int? FolderId { get; set; }
        public VaultSidebarViewModel Sidebar { get; set; } = null!;
        public LoginItemViewModel? LoginItem { get; set; }
        public CardItemViewModel? CardItem { get; set; }
        public NoteItemViewModel? NoteItem { get; set; }
    }
}
