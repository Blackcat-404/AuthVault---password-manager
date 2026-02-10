using System.ComponentModel.DataAnnotations;

namespace PasswordManager.ViewModels.Vault
{
    public class VaultSettingsViewModel
    {
        public VaultSidebarViewModel Sidebar { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FAEmail { get; set; }
        public bool Is2FAEnabled { get; set; }
        public string accountCreatedOn { get; set; } = null!;
        public DateTime? PasswordLastChangeAt { get; set; }
        public int SessionTimeoutMinutes { get; set; }
    }

    public class FAuthenticationEmailViewModel
    {
        public VaultSidebarViewModel? Sidebar { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = null!;
    }

    public class FAuthenticationCodeViewModel
    {
        public VaultSidebarViewModel? Sidebar { get; set; }

        [Required(ErrorMessage = "Code is required")]
        [MaxLength(6,ErrorMessage = "Maximum length is 6")]
        public string Code { get; set; } = null!;
    }

    public class ChangeMasterPasswordViewModel
    {
        public VaultSidebarViewModel? Sidebar { get; set; }

        [Required(ErrorMessage = "Enter password")]
        [MinLength(8,ErrorMessage = "Minimum length is 8")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Enter password")]
        [Compare(nameof(NewPassword),ErrorMessage = "Passwords do not match")]
        public string VerifyPassword { get; set; } = null!;
    }

    public class DeleteAccountViewModel
    {
        public VaultSidebarViewModel? Sidebar { get; set; }

        [Required(ErrorMessage = "Password is required for deleting account")]
        public string Password { get; set; } = null!;
    }

    public class DeleteAccountConfirmationViewModel
    {
        public VaultSidebarViewModel? Sidebar { get; set; }
    }
}
