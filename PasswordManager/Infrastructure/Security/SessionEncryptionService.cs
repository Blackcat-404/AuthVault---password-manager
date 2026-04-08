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

        public void SetPendingEncryptionKey(int userId, byte[] encryptionKey)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Set($"PendingEncryptionKey_{userId}", encryptionKey);
        }

        public void ActivatePendingEncryptionKey(int userId)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return;

            var pendingKey = session.Get($"PendingEncryptionKey_{userId}");
            if (pendingKey != null)
            {
                session.Set($"EncryptionKey_{userId}", pendingKey);
                session.SetInt32("CurrentUserId", userId);
                Array.Clear(pendingKey, 0, pendingKey.Length);
                session.Remove($"PendingEncryptionKey_{userId}");
            }
        }

        public void SetPending2FAUserId(int userId)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.SetInt32("Pending2FAUserId", userId);
        }

        public int? GetPending2FAUserId()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.GetInt32("Pending2FAUserId");
        }

        public void ClearPending2FAUserId()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove("Pending2FAUserId");
        }
    }
}
