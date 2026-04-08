namespace PasswordManager.Application.Security
{
    public interface ISessionEncryptionService
    {
        void SetEncryptionKey(int userId, byte[] encryptionKey);
        byte[]? GetEncryptionKey(int userId);
        void ClearEncryptionKey(int userId);
        void ClearAllKeys();

        void SetPendingEncryptionKey(int userId, byte[] encryptionKey);
        void ActivatePendingEncryptionKey(int userId);
        void SetPending2FAUserId(int userId);
        int? GetPending2FAUserId();
        void ClearPending2FAUserId();
    }
}
