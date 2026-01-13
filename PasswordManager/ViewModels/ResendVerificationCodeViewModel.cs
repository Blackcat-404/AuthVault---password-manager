using System.ComponentModel.DataAnnotations;

namespace PasswordManager.ViewModels
{
    public class ResendVerificationCodeViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

    }
}
