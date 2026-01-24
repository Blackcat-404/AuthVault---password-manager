using PasswordManager.Application.Vault;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Data;

namespace PasswordManager.Infrastructure.Vault
{
    public class DeleteItemService : IDeleteItemService
    {

        private readonly AppDbContext _db;

        public DeleteItemService(AppDbContext db)
        {
            _db = db;
        }

        public async Task DeleteLoginItemAsync(int userId, int itemId)
        {
            var loginItem = await _db.LoginData
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == itemId);

            _db.LoginData.Remove(loginItem!);
            await _db.SaveChangesAsync();
        }


        public async Task DeleteCardItemAsync(int userId, int itemId)
        {

            var cardItem = await _db.CardData
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == itemId);


            _db.CardData.Remove(cardItem!);
            await _db.SaveChangesAsync();
        }


        public async Task DeleteNoteItemAsync(int userId, int itemId)
        {

            var noteItem = await _db.NoteData
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == itemId);


            _db.NoteData.Remove(noteItem!);
            await _db.SaveChangesAsync();
        }
    }
}
