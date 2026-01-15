namespace PasswordManager.Application.Account.Email
{
    public class EmailVerificationDto
    {
        public string Email { get; set; } = null!;
        public int VerificationCode { get; set; }
    }
}
