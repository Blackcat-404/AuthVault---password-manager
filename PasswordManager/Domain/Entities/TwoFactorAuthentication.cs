namespace PasswordManager.Domain.Entities
{
    public class TwoFactorAuthentication
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string? Email { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenExpiresAt { get; set; }
        public bool IsEnabled { get; set; } = false;
        public DateTime? LinkedAt { get; set; }
    }
}
