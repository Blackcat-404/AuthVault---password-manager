using System.ComponentModel.DataAnnotations;

namespace PasswordManager.ViewModels
{
    public class EmailVerificationViewModel
    {
        [Required(ErrorMessage = "Verification code is required")]
        public int VerificationCode { get; set; }
    }
}
