// Services/IFileEncryptionService.cs
namespace Contract_Monthly_Claim_System.Services
{
    public interface IFileEncryptionService
    {
        byte[] EncryptStreamToBytes(Stream inputStream);
        void DecryptBytesToStream(byte[] encryptedContent, Stream outputStream);
    }
}