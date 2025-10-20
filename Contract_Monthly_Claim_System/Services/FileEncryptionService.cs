// Services/FileEncryptionService.cs
using System.Security.Cryptography;

namespace Contract_Monthly_Claim_System.Services
{
    public class FileEncryptionService : IFileEncryptionService
    {
        // In a real application, these should come from a secure configuration source (e.g., Azure Key Vault).
        // For this POE, they are hardcoded as per the project context.
        private readonly byte[] _key = new byte[32] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 };
        private readonly byte[] _iv = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

        public byte[] EncryptStreamToBytes(Stream inputStream)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                using (var outputStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        inputStream.CopyTo(cryptoStream);
                    } // The cryptoStream is disposed here, which finalizes the encryption.
                    return outputStream.ToArray();
                }
            }
        }

        public void DecryptBytesToStream(byte[] encryptedContent, Stream outputStream)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                using (var inputStream = new MemoryStream(encryptedContent))
                using (var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cryptoStream.CopyTo(outputStream);
                }
            }
        }
    }
}