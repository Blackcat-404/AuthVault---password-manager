using System.ComponentModel.DataAnnotations;

namespace PasswordManager.ViewModels.Vault
{
    public class FolderViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Folder name is required")]
        [StringLength(100, ErrorMessage = "Folder name cannot exceed 100 characters")]
        public string Name { get; set; } = null!;

        [StringLength(200)]
        public string? Description { get; set; }

        public string Color { get; set; } = "blue";

        public DateTime CreatedAt { get; set; }
    }
}