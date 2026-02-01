using PasswordManager.Application.Security;
using Microsoft.AspNetCore.Http;

namespace PasswordManager.Infrastructure.Security
{
    public class SessionEncryptionService : ISessionEncryptionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionEncryptionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetEncryptionKey(int userId, byte[] encryptionKey)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.Set($"EncryptionKey_{userId}", encryptionKey);
                session.SetInt32("CurrentUserId", userId);
            }
        }

        public byte[]? GetEncryptionKey(int userId)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            byte[]? key = session.Get($"EncryptionKey_{userId}");
            return key;
        }

        public void ClearEncryptionKey(int userId)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var key = session.Get($"EncryptionKey_{userId}");
                if (key != null)
                {
                    Array.Clear(key, 0, key.Length);
                }
                session.Remove($"EncryptionKey_{userId}");
                session.Remove("CurrentUserId");
            }
        }

        public void ClearAllKeys()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var userId = session.GetInt32("CurrentUserId");
                if (userId.HasValue)
                {
                    ClearEncryptionKey(userId.Value);
                }
                session.Clear();
            }
        }
    }
}
