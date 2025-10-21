using Contract_Monthly_Claim_System.Services;
using System.Text;

namespace Contract_Monthly_Claim_System_Unit_Test
{
    public class FileEncryptionServiceTests
    {
        [Fact]
        public void EncryptStreamToBytes_And_DecryptBytesToStream_ShouldRoundtripSuccessfully()
        {
            // Arrange
            var sut = new FileEncryptionService();
            var originalText = "This is a highly confidential contract document.";
            var originalData = Encoding.UTF8.GetBytes(originalText);
            using var inputStream = new MemoryStream(originalData);

            // Act
            // 1. Encrypt
            var encryptedData = sut.EncryptStreamToBytes(inputStream);

            // 2. Decrypt
            using var outputStream = new MemoryStream();
            sut.DecryptBytesToStream(encryptedData, outputStream);
            var decryptedData = outputStream.ToArray();
            var decryptedText = Encoding.UTF8.GetString(decryptedData);

            // Assert
            Assert.NotNull(encryptedData);
            Assert.NotEmpty(encryptedData);
            Assert.NotEqual(originalData, encryptedData); // Encrypted data should be different

            Assert.Equal(originalData, decryptedData); // Decrypted data must match original
            Assert.Equal(originalText, decryptedText);
        }
    }
}