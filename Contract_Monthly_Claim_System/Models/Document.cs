// Models/Document.cs
namespace Contract_Monthly_Claim_System.Models
{
    public class Document
    {
        public int Id { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public required byte[] EncryptedContent { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;

        // Foreign Key
        public int ClaimId { get; set; }
    }
}