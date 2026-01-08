namespace PasswordManager.Domain.Entities
{
    public class Folder : BaseEntity
    {
        public string Name { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public ICollection<LoginData> LoginItems { get; set; } = new List<LoginData>();
        public ICollection<CardData> CardItems { get; set; } = new List<CardData>();
        public ICollection<NoteData> NoteItems { get; set; } = new List<NoteData>();
    }
}
