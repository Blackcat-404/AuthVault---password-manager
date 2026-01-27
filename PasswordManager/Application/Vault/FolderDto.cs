namespace PasswordManager.Application.Vault
{
    public class FolderDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string Color { get; set; } = null!;
    }
}
