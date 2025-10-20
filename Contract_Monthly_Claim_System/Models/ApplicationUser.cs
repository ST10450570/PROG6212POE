// Models/ApplicationUser.cs
using System.ComponentModel.DataAnnotations;

namespace Contract_Monthly_Claim_System.Models
{
    public class ApplicationUser
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public UserRole Role { get; set; }
        public string Initials => string.Concat(FullName.Split(' ').Select(n => n[0])).ToUpper();
        public required string department { get; set; }
    }
}