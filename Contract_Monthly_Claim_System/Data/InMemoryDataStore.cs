
using Contract_Monthly_Claim_System.Models;

namespace Contract_Monthly_Claim_System.Data
{
 
    public class InMemoryDataStore
    {
        public List<ApplicationUser> Users { get; } = new List<ApplicationUser>();
        public List<Claim> Claims { get; } = new List<Claim>();
        public List<Document> Documents { get; } = new List<Document>();

        private int _nextClaimId = 1;
        private int _nextDocumentId = 1;

        public int GetNextClaimId() => _nextClaimId++;
        public int GetNextDocumentId() => _nextDocumentId++;

        public void SeedInitialData()
        {
            if (Users.Any()) return;

            // --- Seed Users ---
            Users.AddRange(new List<ApplicationUser>
            {
                new ApplicationUser { Id = 1, FullName = "Chuma Makhathini", Email = "lecturer@IIEMSA.com", Role = UserRole.Lecturer , department = "Computer Science"},
                new ApplicationUser { Id = 2, FullName = "Muzi Sithole", Email = "coordinator@IIEMSA.com", Role = UserRole.Coordinator ,  department ="Head of Cumputer Science"},
                new ApplicationUser { Id = 3, FullName = "Ouma Stella", Email = "manager@IIEMSA.com", Role = UserRole.Manager , department = "Head of School Of IT"}
            });
        }
    }
}