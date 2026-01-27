namespace PasswordManager.ViewModels.Vault
{
    public class VaultSidebarViewModel
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int CountAllItems { get; set; }
        public List<FolderViewModel> Folders { get; set; } = new();
    }
}
