using System.ComponentModel.DataAnnotations;

namespace PasswordManager.ViewModels
{
    public class VaultItemViewModel
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Username { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public string? Url { get; set; }

        public string? Notes { get; set; }

        public string? FolderId { get; set; }

        public string? ItemType { get; set; } // "Login", "Card", "Note"

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}