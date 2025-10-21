using Contract_Monthly_Claim_System.Models;

namespace Contract_Monthly_Claim_System_Unit_Test
{
    public class ClaimModelTests
    {
        private Claim CreateClaim(ClaimStatus status)
        {
            // Required properties must be set
            return new Claim
            {
                Id = 1,
                ClaimNumber = "C-001",
                WorkDescription = "Test",
                HoursWorked = 1,
                HourlyRate = 1,
                TotalAmount = 1,
                UserId = 1,
                Status = status
            };
        }

        [Theory]
        [InlineData(ClaimStatus.Draft, true)]
        [InlineData(ClaimStatus.Returned, true)]
        [InlineData(ClaimStatus.Submitted, false)]
        [InlineData(ClaimStatus.Verified, false)]
        [InlineData(ClaimStatus.Approved, false)]
        [InlineData(ClaimStatus.Rejected, false)]
        public void CanEdit_ShouldBeCorrectForStatus(ClaimStatus status, bool expected)
        {
            // Arrange
            var claim = CreateClaim(status);

            // Act
            var canEdit = claim.CanEdit;

            // Assert
            Assert.Equal(expected, canEdit);
        }

        [Theory]
        [InlineData(ClaimStatus.Submitted, true)]
        [InlineData(ClaimStatus.Draft, false)]
        [InlineData(ClaimStatus.Returned, false)]
        [InlineData(ClaimStatus.Verified, false)]
        [InlineData(ClaimStatus.Approved, false)]
        [InlineData(ClaimStatus.Rejected, false)]
        public void CanVerify_ShouldBeCorrectForStatus(ClaimStatus status, bool expected)
        {
            // Arrange
            var claim = CreateClaim(status);

            // Act
            var canVerify = claim.CanVerify;

            // Assert
            Assert.Equal(expected, canVerify);
        }

        [Theory]
        [InlineData(ClaimStatus.Verified, true)]
        [InlineData(ClaimStatus.Draft, false)]
        [InlineData(ClaimStatus.Returned, false)]
        [InlineData(ClaimStatus.Submitted, false)]
        [InlineData(ClaimStatus.Approved, false)]
        [InlineData(ClaimStatus.Rejected, false)]
        public void CanApprove_ShouldBeCorrectForStatus(ClaimStatus status, bool expected)
        {
            // Arrange
            var claim = CreateClaim(status);

            // Act
            var canApprove = claim.CanApprove;

            // Assert
            Assert.Equal(expected, canApprove);
        }

        [Theory]
        [InlineData(ClaimStatus.Submitted, true)]
        [InlineData(ClaimStatus.Verified, true)]
        [InlineData(ClaimStatus.Draft, false)]
        [InlineData(ClaimStatus.Returned, false)]
        [InlineData(ClaimStatus.Approved, false)]
        [InlineData(ClaimStatus.Rejected, false)]
        public void CanReject_ShouldBeCorrectForStatus(ClaimStatus status, bool expected)
        {
            // Arrange
            var claim = CreateClaim(status);

            // Act
            var canReject = claim.CanReject;

            // Assert
            Assert.Equal(expected, canReject);
        }

        [Theory]
        [InlineData(ClaimStatus.Submitted, true)]
        [InlineData(ClaimStatus.Draft, false)]
        [InlineData(ClaimStatus.Returned, false)]
        [InlineData(ClaimStatus.Verified, false)]
        [InlineData(ClaimStatus.Approved, false)]
        [InlineData(ClaimStatus.Rejected, false)]
        public void CanReturn_ShouldBeCorrectForStatus(ClaimStatus status, bool expected)
        {
            // Arrange
            var claim = CreateClaim(status);

            // Act
            var canReturn = claim.CanReturn;

            // Assert
            Assert.Equal(expected, canReturn);
        }

        [Fact]
        public void CanSubmit_WhenDraftAndHasDocument_ShouldBeTrue()
        {
            // Arrange
            var claim = CreateClaim(ClaimStatus.Draft);
            claim.Documents.Add(new Document { Id = 1, ClaimId = 1, FileName = "doc.pdf", ContentType = "app/pdf", EncryptedContent = new byte[0] });

            // Act & Assert
            Assert.True(claim.CanSubmit);
        }

        [Fact]
        public void CanSubmit_WhenReturnedAndHasDocument_ShouldBeTrue()
        {
            // Arrange
            var claim = CreateClaim(ClaimStatus.Returned);
            claim.Documents.Add(new Document { Id = 1, ClaimId = 1, FileName = "doc.pdf", ContentType = "app/pdf", EncryptedContent = new byte[0] });

            // Act & Assert
            Assert.True(claim.CanSubmit);
        }

        
        [Fact]
        public void CanSubmit_WhenDraftAndNoDocument_ShouldBeFalse()
        {
            // Arrange
            var claim = CreateClaim(ClaimStatus.Draft);
            // No document added

            // Act & Assert
            Assert.False(claim.CanSubmit);
        }

        [Fact]
        public void CanSubmit_WhenSubmittedAndHasDocument_ShouldBeFalse()
        {
            // Arrange
            var claim = CreateClaim(ClaimStatus.Submitted);
            claim.Documents.Add(new Document { Id = 1, ClaimId = 1, FileName = "doc.pdf", ContentType = "app/pdf", EncryptedContent = new byte[0] });

            // Act & Assert
            Assert.False(claim.CanSubmit);
        }
    }
}