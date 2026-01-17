using Microsoft.EntityFrameworkCore;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Folder> Folders => Set<Folder>();
        public DbSet<LoginData> LoginData => Set<LoginData>();
        public DbSet<CardData> CardData => Set<CardData>();
        public DbSet<NoteData> NoteData => Set<NoteData>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User -> Folder (CASCADE)
            modelBuilder.Entity<Folder>()
                .HasOne(f => f.User)
                .WithMany(u => u.Folders)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> LoginData (NO ACTION)
            modelBuilder.Entity<LoginData>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // User -> CardData (NO ACTION)
            modelBuilder.Entity<CardData>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // User -> NoteData (NO ACTION)
            modelBuilder.Entity<NoteData>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
