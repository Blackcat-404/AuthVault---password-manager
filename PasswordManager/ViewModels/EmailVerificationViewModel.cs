using System.ComponentModel.DataAnnotations;

namespace PasswordManager.ViewModels
{
    public class EmailVerificationViewModel
    {
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Verification code is required")]
        public int VerificationCode { get; set; }
    }
}
