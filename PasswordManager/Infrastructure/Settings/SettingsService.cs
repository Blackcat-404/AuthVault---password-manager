using Microsoft.EntityFrameworkCore;
using PasswordManager.Data;
using PasswordManager.Domain.Entities;
using PasswordManager.Infrastructure.Email;
using PasswordManager.Infrastructure.Security;
using System.Text;

namespace PasswordManager.Infrastructure.Settings
{
   
    public class SettingsService 
    {
        private readonly AppDbContext _db;
        private readonly EmailService _emailService;
        public SettingsService(AppDbContext db,EmailService emailService) 
        {
            _db = db;
            _emailService = emailService;
        }

        public async Task Add2FAAsync(int userId,string email)
        {
            int code = VerificationCodeGenerator.Generate();
            var expiresAt = DateTime.UtcNow.AddMinutes(5);

            var twoFa = await _db.TwoFactorAuthentications
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (twoFa == null)
            {
                twoFa = new TwoFactorAuthentication
                {
                    UserId = userId,
                    Code = code.ToString(),
                    ExpiresAt = expiresAt
                };

                _db.TwoFactorAuthentications.Add(twoFa);
            }
            else
            {
                twoFa.Code = code.ToString();
                twoFa.ExpiresAt = expiresAt;
            }

            await _db.SaveChangesAsync();
            await _emailService.SendAsync(
                email,
                "Your verification code",
                $"Your verification code for 2FA is: {code}\n\nIt expires in 5 minutes."
            );
        }

        public async Task<bool> Verify2FACodeAsync(int userId, string code, string email)
        {
            var record = await _db.TwoFactorAuthentications
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync();

            if (record == null)
            {
                return false;
            }

            if (record.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            if (record.Code != code)
            {
                return false;
            }

            record.Email = email;
            record.Code = null;
            record.ExpiresAt = null;
            record.LinkedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task Set2FAStatement(int userId, bool statement)
        {
            var record = await _db.TwoFactorAuthentications
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync();

            if (record == null)
            {
                return;
            }
            
            record.IsEnabled = statement;
            await _db.SaveChangesAsync();
        }

        public async Task ChangeMasterPassword(int userId,string password)
        {
            var user = await _db.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();

            if (user == null)
            {
                return;
            }

            user.PasswordHash = PasswordHasher.Hash(password);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> PasswordVerifyAsync(int userId, string password)
        {
            var user = await _db.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();

            if (user == null)
            {
                return false;
            }
            return PasswordHasher.Verify(password, user.PasswordHash);
        }

        public async Task<bool> DeleteAccountAsync(int userId)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                await _db.LoginData
                    .Where(x => x.UserId == userId)
                    .ExecuteDeleteAsync();

                await _db.CardData
                    .Where(x => x.UserId == userId)
                    .ExecuteDeleteAsync();

                await _db.NoteData
                    .Where(x => x.UserId == userId)
                    .ExecuteDeleteAsync();

                await _db.Folders
                    .Where(x => x.UserId == userId)
                    .ExecuteDeleteAsync();

                await _db.TwoFactorAuthentications
                    .Where(x => x.UserId == userId)
                    .ExecuteDeleteAsync();

                var user = await _db.Users.FindAsync(userId);
                if (user != null)
                    _db.Users.Remove(user);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<string> ExportUserDataAsTextAsync(int userId)
        {
            try
            {
                var sb = new StringBuilder();

                var user = await _db.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new
                    {
                        u.Id,
                        u.Login,
                        u.Email,
                        u.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                    throw new Exception("User not found");

                sb.AppendLine("=========================================");
                sb.AppendLine("=============== USER INFO ===============");
                sb.AppendLine("=========================================");
                sb.AppendLine();
                sb.AppendLine($"Login: {user.Login}");
                sb.AppendLine($"Email: {user.Email}");
                sb.AppendLine($"Created At: {user.CreatedAt:yyyy-MM-dd HH:mm}");
                sb.AppendLine();

                // ===== FOLDERS =====
                var folders = await _db.Folders
                    .Where(f => f.UserId == userId)
                    .ToListAsync();

                sb.AppendLine("=========================================");
                sb.AppendLine("=============== FOLDERS =================");
                sb.AppendLine("=========================================");
                sb.AppendLine(folders.Any() ? "" : "No folders");

                foreach (var folder in folders)
                {
                    sb.AppendLine($"--- {folder.Name}");
                    sb.AppendLine($"---Created at:{folder.CreatedAt:yyyy-MM-dd HH:mm}");
                    if (!string.IsNullOrWhiteSpace(folder.Description))
                        sb.AppendLine($"---Description: {folder.Description}");
                    sb.AppendLine();
                }

                sb.AppendLine();

                // ===== LOGIN DATA =====
                var logins = await _db.LoginData
                    .Where(l => l.UserId == userId)
                    .ToListAsync();

                sb.AppendLine();
                sb.AppendLine("=========================================");
                sb.AppendLine("================ LOGINS =================");
                sb.AppendLine("=========================================");
                sb.AppendLine(logins.Any() ? "" : "No login entries");

                foreach (var login in logins)
                {
                    sb.AppendLine($"--- Title: {login.Title}");
                    sb.AppendLine($"--- Folder: {login.Folder?.Name ?? "No folder"}");
                    sb.AppendLine($"--- Login: "); //TODO
                    sb.AppendLine($"--- WEB URL: {login.WebURL ?? "No URL"}");
                    sb.AppendLine($"--- Created at: {login.CreatedAt:yyyy-MM-dd}");
                    sb.AppendLine($"--- Note: "); //TODO
                    sb.AppendLine();
                }

                // ===== CARDS =====
                var cards = await _db.CardData
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                sb.AppendLine();
                sb.AppendLine("=========================================");
                sb.AppendLine("================= CARDS =================");
                sb.AppendLine("=========================================");
                sb.AppendLine(cards.Any() ? "" : "No cards");

                foreach (var card in cards)
                {
                    sb.AppendLine($"--- Title: {card.Title}");
                    sb.AppendLine($"--- Cardholder name: {card.CardholderName}");
                    sb.AppendLine($"--- Folder: {card.Folder?.Name ?? "No folder"}");
                    sb.AppendLine($"--- Card number: "); //TODO
                    sb.AppendLine($"--- Expire month: "); //TODO
                    sb.AppendLine($"--- Expire year: "); //TODO
                    sb.AppendLine($"--- Created at: {card.CreatedAt:yyyy-MM-dd}");
                    sb.AppendLine($"--- Note: "); //TODO
                    sb.AppendLine();
                }

                // ===== NOTES =====
                var notes = await _db.NoteData
                    .Where(n => n.UserId == userId)
                    .ToListAsync();

                sb.AppendLine();
                sb.AppendLine("=========================================");
                sb.AppendLine("================= NOTES =================");
                sb.AppendLine("=========================================");
                sb.AppendLine(notes.Any() ? "" : "No notes");

                foreach (var note in notes)
                {
                    sb.AppendLine($"--- Title: {note.Title}");
                    sb.AppendLine($"--- Folder: {note.Folder?.Name ?? "No folder"}");
                    sb.AppendLine($"--- Created at: {note.CreatedAt:yyyy-MM-dd}");
                    sb.AppendLine($"--- Content: "); //TODO
                    sb.AppendLine();
                }

                // ===== 2FA =====
                var twoFa = await _db.TwoFactorAuthentications
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                sb.AppendLine();
                sb.AppendLine("=========================================");
                sb.AppendLine("============ TWO FACTOR AUTH ============");
                sb.AppendLine("=========================================");
                if (twoFa == null || !twoFa.IsEnabled)
                {
                    sb.AppendLine("2FA is disabled");
                }
                else
                {
                    sb.AppendLine("2FA is enabled");
                }
                return sb.ToString();
            }
            catch
            {
                return "";
            }
        }

        public async Task<string> ExportUserDataAsMarkdownAsync(int userId)
        {
            try
            {
                var sb = new StringBuilder();

                var user = await _db.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new
                    {
                        u.Id,
                        u.Login,
                        u.Email,
                        u.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                    throw new Exception("User not found");

                sb.AppendLine("# User Info");
                sb.AppendLine();
                sb.AppendLine($"- **Login:** {user.Login}");
                sb.AppendLine($"- **Email:** {user.Email}");
                sb.AppendLine($"- **Created at:** {user.CreatedAt:yyyy-MM-dd HH:mm}");
                sb.AppendLine();


                // ===== FOLDERS =====
                var folders = await _db.Folders
                    .Where(f => f.UserId == userId)
                    .ToListAsync();

                sb.AppendLine("# Folders");
                sb.AppendLine();

                if (!folders.Any())
                {
                    sb.AppendLine("_No folders_");
                }
                else
                {
                    foreach (var folder in folders)
                    {
                        sb.AppendLine($"## {folder.Name}");
                        sb.AppendLine($"- **Created at:** {folder.CreatedAt:yyyy-MM-dd HH:mm}");

                        if (!string.IsNullOrWhiteSpace(folder.Description))
                            sb.AppendLine($"- **Description:** {folder.Description}");

                        sb.AppendLine();
                    }
                }


                // ===== LOGIN DATA =====
                var logins = await _db.LoginData
                    .Where(l => l.UserId == userId)
                    .ToListAsync();

                sb.AppendLine("# Logins");
                sb.AppendLine();

                if (!logins.Any())
                {
                    sb.AppendLine("_No login entries_");
                }
                else
                {
                    foreach (var login in logins)
                    {
                        sb.AppendLine($"## {login.Title}");
                        sb.AppendLine($"- **Folder:** {login.Folder?.Name ?? "No folder"}");
                        sb.AppendLine($"- **Login:** _hidden_"); // TODO
                        sb.AppendLine($"- **Web URL:** {login.WebURL ?? "No URL"}");
                        sb.AppendLine($"- **Created at:** {login.CreatedAt:yyyy-MM-dd}");
                        sb.AppendLine($"- **Note:** _hidden_"); // TODO
                        sb.AppendLine();
                    }
                }


                // ===== CARDS =====
                var cards = await _db.CardData
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                sb.AppendLine("# Cards");
                sb.AppendLine();

                if (!cards.Any())
                {
                    sb.AppendLine("_No cards_");
                }
                else
                {
                    foreach (var card in cards)
                    {
                        sb.AppendLine($"## {card.Title}");
                        sb.AppendLine($"- **Cardholder name:** {card.CardholderName ?? "_hidden_"}");
                        sb.AppendLine($"- **Folder:** {card.Folder?.Name ?? "No folder"}");
                        sb.AppendLine($"- **Card number:** _hidden_"); // TODO
                        sb.AppendLine($"- **Expire month:** _hidden_"); // TODO
                        sb.AppendLine($"- **Expire year:** _hidden_"); // TODO
                        sb.AppendLine($"- **Created at:** {card.CreatedAt:yyyy-MM-dd}");
                        sb.AppendLine($"- **Note:** _hidden_"); // TODO
                        sb.AppendLine();
                    }
                }


                // ===== NOTES =====
                var notes = await _db.NoteData
                    .Where(n => n.UserId == userId)
                    .ToListAsync();

                sb.AppendLine("# Notes");
                sb.AppendLine();

                if (!notes.Any())
                {
                    sb.AppendLine("_No notes_");
                }
                else
                {
                    foreach (var note in notes)
                    {
                        sb.AppendLine($"## {note.Title}");
                        sb.AppendLine($"- **Folder:** {note.Folder?.Name ?? "No folder"}");
                        sb.AppendLine($"- **Created at:** {note.CreatedAt:yyyy-MM-dd}");
                        sb.AppendLine($"- **Content:** _hidden_"); // TODO
                        sb.AppendLine();
                    }
                }


                // ===== 2FA =====
                var twoFa = await _db.TwoFactorAuthentications
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                sb.AppendLine("# Two-Factor Authentication");
                sb.AppendLine();

                sb.AppendLine(twoFa == null || !twoFa.IsEnabled
                    ? "- 2FA is **disabled**"
                    : "- 2FA is **enabled**");

                return sb.ToString();
            }
            catch
            {
                return "";
            }
        }
    }
}
