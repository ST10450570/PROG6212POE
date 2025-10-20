// Data/InMemoryDataStore.cs
using Contract_Monthly_Claim_System.Models;

namespace Contract_Monthly_Claim_System.Data
{
    /// <summary>
    /// Singleton service to act as an in-memory database for the application's lifetime.
    /// </summary>
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
            if (Users.Any()) return; // Data already seeded

            // --- Seed Users ---
            Users.AddRange(new List<ApplicationUser>
            {
                new ApplicationUser { Id = 1, FullName = "David Wilson", Email = "lecturer@university.com", Role = UserRole.Lecturer },
                new ApplicationUser { Id = 2, FullName = "Sarah Johnson", Email = "coordinator@university.com", Role = UserRole.Coordinator },
                new ApplicationUser { Id = 3, FullName = "Michael Brown", Email = "manager@university.com", Role = UserRole.Manager }
            });
        }
    }
}