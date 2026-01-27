using System.ComponentModel.DataAnnotations;

namespace PasswordManager.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email or login is required")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

    }

    public class Login2FAViewModel
    {
        public string Code { get; set; } = null!;
    }
}
