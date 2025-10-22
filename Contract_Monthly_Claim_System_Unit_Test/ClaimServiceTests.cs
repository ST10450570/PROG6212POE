using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.Services;
using Contract_Monthly_Claim_System.ViewModels;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace Contract_Monthly_Claim_System_Unit_Test
{
    public class ClaimServiceTests
    {
        // Helper method to create a mock IFormFile
        private Mock<IFormFile> CreateMockFormFile()
        {
            var mockFile = new Mock<IFormFile>();
            var content = "This is a dummy file.";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.FileName).Returns("test.pdf");
            mockFile.Setup(f => f.ContentType).Returns("application/pdf");
            mockFile.Setup(f => f.Length).Returns(stream.Length);
            return mockFile;
        }

        [Fact]
        public async Task CreateClaimAsync_ShouldCreateClaimAndDocument_AndEncryptFile()
        {
            // Arrange
            var dataStore = new InMemoryDataStore();
            dataStore.SeedInitialData(); // Seeds user ID 1
            var mockEncryptionService = new Mock<IFileEncryptionService>();
            var sut = new ClaimService(dataStore, mockEncryptionService.Object);

            var mockFile = CreateMockFormFile();
            var encryptedData = new byte[] { 1, 2, 3, 4, 5 };
            var userId = 1; // Chuma Makhathini

            var viewModel = new ClaimCreateViewModel
            {
                WorkDescription = "Test Module",
                HoursWorked = 10,
                HourlyRate = 50,
                Notes = "Test notes",
                SupportingDocument = mockFile.Object
            };

            mockEncryptionService.Setup(s => s.EncryptStreamToBytes(It.IsAny<Stream>()))
                                 .Returns(encryptedData);

            // Act
            var result = await sut.CreateClaimAsync(viewModel, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(viewModel.WorkDescription, result.WorkDescription);
            Assert.Equal(500, result.TotalAmount); // 10 * 50
            Assert.Equal(ClaimStatus.Draft, result.Status);
            Assert.Equal(userId, result.UserId);
            Assert.NotNull(result.User);
            Assert.Equal("Chuma Makhathini", result.User.FullName);

            // Verify data store
            Assert.Single(dataStore.Claims);
            Assert.Single(dataStore.Documents);
            var document = dataStore.Documents.First();
            Assert.Equal(result.Id, document.ClaimId);
            Assert.Equal("test.pdf", document.FileName);
            Assert.Equal(encryptedData, document.EncryptedContent);

            // Verify encryption service was called
            mockEncryptionService.Verify(s => s.EncryptStreamToBytes(It.IsAny<Stream>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnClaimWithUserAndDocuments()
        {
            // Arrange
            var dataStore = new InMemoryDataStore();
            dataStore.SeedInitialData();
            var mockEncryptionService = new Mock<IFileEncryptionService>();
            var sut = new ClaimService(dataStore, mockEncryptionService.Object);

            var user = dataStore.Users.First();
            var claim = new Claim { Id = 1, ClaimNumber = "C-001", WorkDescription = "Test", UserId = user.Id, HourlyRate = 1, HoursWorked = 1 };
            var document = new Document { Id = 1, ClaimId = 1, FileName = "doc.pdf", ContentType = "app/pdf", EncryptedContent = new byte[0] };
            
            dataStore.Claims.Add(claim);
            dataStore.Documents.Add(document);

            // Act
            var result = await sut.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(claim.Id, result.Id);
            Assert.NotNull(result.User); // User should be populated
            Assert.Equal(user.FullName, result.User.FullName);
            Assert.Single(result.Documents); // Documents should be populated
            Assert.Equal(document.FileName, result.Documents.First().FileName);
        }

        [Fact]
        public async Task SubmitClaimAsync_WhenDraftWithDocument_ShouldSetStatusToSubmitted()
        {
            // Arrange
            var dataStore = new InMemoryDataStore();
            dataStore.SeedInitialData();
            var mockEncryptionService = new Mock<IFileEncryptionService>();
            var sut = new ClaimService(dataStore, mockEncryptionService.Object);

            var user = dataStore.Users.First();
            var claim = new Claim { Id = 1, ClaimNumber = "C-001", WorkDescription = "Test", UserId = user.Id, Status = ClaimStatus.Draft, HourlyRate = 1, HoursWorked = 1 };
            var document = new Document { Id = 1, ClaimId = 1, FileName = "doc.pdf", ContentType = "app/pdf", EncryptedContent = new byte[0] };
            
            dataStore.Claims.Add(claim);
            dataStore.Documents.Add(document); // Add document so CanSubmit is true

            // Act
            await sut.SubmitClaimAsync(1, user.Id);

            // Assert
            var updatedClaim = dataStore.Claims.First();
            Assert.Equal(ClaimStatus.Submitted, updatedClaim.Status);
            Assert.NotNull(updatedClaim.SubmittedDate);
        }

        [Fact]
        public async Task SubmitClaimAsync_WhenNoDocument_ShouldThrowException()
        {
            // Arrange
            var dataStore = new InMemoryDataStore();
            dataStore.SeedInitialData();
            var mockEncryptionService = new Mock<IFileEncryptionService>();
            var sut = new ClaimService(dataStore, mockEncryptionService.Object);

            var user = dataStore.Users.First();
            var claim = new Claim { Id = 1, ClaimNumber = "C-001", WorkDescription = "Test", UserId = user.Id, Status = ClaimStatus.Draft, HourlyRate = 1, HoursWorked = 1 };
            
            dataStore.Claims.Add(claim); // No document

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SubmitClaimAsync(1, user.Id));
            Assert.Equal("Claim not found or cannot be submitted.", ex.Message);
        }

        [Fact]
        public async Task VerifyClaimAsync_WhenSubmitted_ShouldSetStatusToVerified()
        {
            // Arrange
            var dataStore = new InMemoryDataStore();
            dataStore.SeedInitialData();
            var mockEncryptionService = new Mock<IFileEncryptionService>();
            var sut = new ClaimService(dataStore, mockEncryptionService.Object);

            var claim = new Claim { Id = 1, ClaimNumber = "C-001", WorkDescription = "Test", UserId = 1, Status = ClaimStatus.Submitted, HourlyRate = 1, HoursWorked = 1 };
            dataStore.Claims.Add(claim);

            var coordinatorId = 2;
            var comments = "Looks good.";

            // Act
            await sut.VerifyClaimAsync(1, comments, coordinatorId);

            // Assert
            Assert.Equal(ClaimStatus.Verified, claim.Status);
            Assert.Equal(comments, claim.ReviewerComments);
        }

        [Fact]
        public async Task ApproveClaimAsync_WhenVerified_ShouldSetStatusToApproved()
        {
            // Arrange
            var dataStore = new InMemoryDataStore();
            dataStore.SeedInitialData();
            var mockEncryptionService = new Mock<IFileEncryptionService>();
            var sut = new ClaimService(dataStore, mockEncryptionService.Object);

            var claim = new Claim { Id = 1, ClaimNumber = "C-001", WorkDescription = "Test", UserId = 1, Status = ClaimStatus.Verified, HourlyRate = 1, HoursWorked = 1 };
            dataStore.Claims.Add(claim);

            var managerId = 3;
            var comments = "Approved.";

            // Act
            await sut.ApproveClaimAsync(1, comments, managerId);

            // Assert
            Assert.Equal(ClaimStatus.Approved, claim.Status);
            Assert.Equal(comments, claim.ReviewerComments);
        }

        [Fact]
        public async Task RejectClaimAsync_WhenVerified_ShouldSetStatusToRejected()
        {
            // Arrange
            var dataStore = new InMemoryDataStore();
            dataStore.SeedInitialData();
            var mockEncryptionService = new Mock<IFileEncryptionService>();
            var sut = new ClaimService(dataStore, mockEncryptionService.Object);

            var claim = new Claim { Id = 1, ClaimNumber = "C-001", WorkDescription = "Test", UserId = 1, Status = ClaimStatus.Verified, HourlyRate = 1, HoursWorked = 1 };
            dataStore.Claims.Add(claim);

            var reviewerId = 3;
            var reason = "Incorrect hours.";

            // Act
            await sut.RejectClaimAsync(1, reason, reviewerId);

            // Assert
            Assert.Equal(ClaimStatus.Rejected, claim.Status);
            Assert.Equal(reason, claim.RejectionReason);
        }

        [Fact]
        public async Task ReturnClaimAsync_WhenSubmitted_ShouldSetStatusToReturned()
        {
            // Arrange
            var dataStore = new InMemoryDataStore();
            dataStore.SeedInitialData();
            var mockEncryptionService = new Mock<IFileEncryptionService>();
            var sut = new ClaimService(dataStore, mockEncryptionService.Object);

            var claim = new Claim { Id = 1, ClaimNumber = "C-001", WorkDescription = "Test", UserId = 1, Status = ClaimStatus.Submitted, HourlyRate = 1, HoursWorked = 1 };
            dataStore.Claims.Add(claim);

            var coordinatorId = 2;
            var comments = "Please add more details.";

            // Act
            await sut.ReturnClaimAsync(1, comments, coordinatorId);

            // Assert
            Assert.Equal(ClaimStatus.Returned, claim.Status);
            Assert.Equal(comments, claim.ReviewerComments);
        }

        [Fact]
        public async Task UpdateClaimAsync_WhenReturned_ShouldUpdateAndSetStatusToDraft()
        {
            // Arrange
            var dataStore = new InMemoryDataStore();
            dataStore.SeedInitialData();
            var mockEncryptionService = new Mock<IFileEncryptionService>();
            var sut = new ClaimService(dataStore, mockEncryptionService.Object);

            var userId = 1;
            var claim = new Claim 
            { 
                Id = 1, 
                ClaimNumber = "C-001", 
                WorkDescription = "Old Desc", 
                UserId = userId, 
                Status = ClaimStatus.Returned, 
                HourlyRate = 10, 
                HoursWorked = 10, 
                TotalAmount = 100,
                ReviewerComments = "Please fix."
            };
            dataStore.Claims.Add(claim);

            var viewModel = new ClaimEditViewModel
            {
                WorkDescription = "Updated Description",
                HoursWorked = 20,
                HourlyRate = 20,
                Notes = "Updated notes"
            };

            // Act
            await sut.UpdateClaimAsync(1, viewModel, userId);

            // Assert
            Assert.Equal(viewModel.WorkDescription, claim.WorkDescription);
            Assert.Equal(viewModel.HoursWorked, claim.HoursWorked);
            Assert.Equal(400, claim.TotalAmount); // 20 * 20
            Assert.Equal(ClaimStatus.Draft, claim.Status); // Status should revert to Draft
            Assert.Null(claim.ReviewerComments); // Reviewer comments should be cleared
            Assert.True(claim.UpdatedDate > DateTime.MinValue);
        }

        [Fact]
        public async Task UpdateClaimAsync_WhenSubmitted_ShouldThrowException()
        {
            // Arrange
            var dataStore = new InMemoryDataStore();
            dataStore.SeedInitialData();
            var mockEncryptionService = new Mock<IFileEncryptionService>();
            var sut = new ClaimService(dataStore, mockEncryptionService.Object);

            var userId = 1;
            var claim = new Claim { Id = 1, ClaimNumber = "C-001", WorkDescription = "Test", UserId = userId, Status = ClaimStatus.Submitted, HourlyRate = 1, HoursWorked = 1 };
            dataStore.Claims.Add(claim);
            
            var viewModel = new ClaimEditViewModel { WorkDescription = "Updated" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.UpdateClaimAsync(1, viewModel, userId));
            Assert.Equal("Only draft and returned claims can be edited.", ex.Message);
        }

        [Fact]
        public async Task GetPendingCoordinatorClaimsAsync_ShouldOnlyReturnSubmitted()
        {
            // Arrange
            var dataStore = new InMemoryDataStore();
            dataStore.SeedInitialData();
            var mockEncryptionService = new Mock<IFileEncryptionService>();
            var sut = new ClaimService(dataStore, mockEncryptionService.Object);

            dataStore.Claims.AddRange(new List<Claim>
            {
                new Claim { Id = 1, ClaimNumber = "C-001", WorkDescription = "Test", UserId = 1, Status = ClaimStatus.Draft, HourlyRate = 1, HoursWorked = 1 },
                new Claim { Id = 2, ClaimNumber = "C-002", WorkDescription = "Test", UserId = 1, Status = ClaimStatus.Submitted, SubmittedDate = DateTime.Now.AddMinutes(-10), HourlyRate = 1, HoursWorked = 1 },
                new Claim { Id = 3, ClaimNumber = "C-003", WorkDescription = "Test", UserId = 1, Status = ClaimStatus.Submitted, SubmittedDate = DateTime.Now.AddMinutes(-5), HourlyRate = 1, HoursWorked = 1 },
                new Claim { Id = 4, ClaimNumber = "C-004", WorkDescription = "Test", UserId = 1, Status = ClaimStatus.Verified, HourlyRate = 1, HoursWorked = 1 },
                new Claim { Id = 5, ClaimNumber = "C-005", WorkDescription = "Test", UserId = 1, Status = ClaimStatus.Returned, HourlyRate = 1, HoursWorked = 1 }
            });

            // Act
            var results = (await sut.GetPendingCoordinatorClaimsAsync()).ToList();

            // Assert
            Assert.Equal(2, results.Count);
            Assert.True(results.All(c => c.Status == ClaimStatus.Submitted));
            Assert.Equal(2, results[0].Id); // Should be ordered by SubmittedDate ascending
            Assert.Equal(3, results[1].Id);
        }

        [Fact]
        public async Task GetTotalApprovedAmountAsync_ShouldSumCorrectly()
        {
            // Arrange
            var dataStore = new InMemoryDataStore();
            dataStore.SeedInitialData();
            var mockEncryptionService = new Mock<IFileEncryptionService>();
            var sut = new ClaimService(dataStore, mockEncryptionService.Object);
            var userId = 1;

             dataStore.Claims.AddRange(new List<Claim>
            {
                new Claim { Id = 1, UserId = userId, Status = ClaimStatus.Approved, TotalAmount = 100, HourlyRate = 1, HoursWorked = 1, ClaimNumber = "C-001", WorkDescription = "Test" },
                new Claim { Id = 2, UserId = userId, Status = ClaimStatus.Approved, TotalAmount = 150, HourlyRate = 1, HoursWorked = 1, ClaimNumber = "C-002", WorkDescription = "Test" },
                new Claim { Id = 3, UserId = userId, Status = ClaimStatus.Rejected, TotalAmount = 200, HourlyRate = 1, HoursWorked = 1, ClaimNumber = "C-003", WorkDescription = "Test" },
                new Claim { Id = 4, UserId = 2, Status = ClaimStatus.Approved, TotalAmount = 1000, HourlyRate = 1, HoursWorked = 1, ClaimNumber = "C-004", WorkDescription = "Test" }
            });

            // Act
            var result = await sut.GetTotalApprovedAmountAsync(userId);

            // Assert
            Assert.Equal(250, result);
        }
    }
}