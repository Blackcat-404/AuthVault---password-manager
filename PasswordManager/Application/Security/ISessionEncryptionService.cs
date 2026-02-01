namespace PasswordManager.Application.Security
{
    public interface ISessionEncryptionService
    {
        void SetEncryptionKey(int userId, byte[] encryptionKey);
        byte[]? GetEncryptionKey(int userId);
        void ClearEncryptionKey(int userId);
        void ClearAllKeys();
    }
}
