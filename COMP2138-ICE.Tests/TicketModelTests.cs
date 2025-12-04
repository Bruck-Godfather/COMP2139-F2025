using System.ComponentModel.DataAnnotations;
using COMP2138_ICE.Models;
using Xunit;

namespace COMP2138_ICE.Tests
{
    public class TicketModelTests
    {
        [Fact]
        public void Ticket_DefaultIsUsed_IsFalse()
        {
            // Arrange & Act
            var ticket = new Ticket
            {
                PurchaseId = 1,
                QRCodeData = "QR123",
                TicketNumber = "TKT-001"
            };

            // Assert
            Assert.False(ticket.IsUsed);
        }

        [Fact]
        public void Ticket_RequiredFields_AreValidated()
        {
            // Arrange
            var ticket = new Ticket
            {
                // Missing required fields - PurchaseId defaults to 0
                QRCodeData = "",
                TicketNumber = ""
            };

            // Act
            var context = new ValidationContext(ticket);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(ticket, context, results, true);

            // Assert
            Assert.False(isValid);
            // Check that required fields are validated
            Assert.True(results.Any(r => r.MemberNames.Contains("QRCodeData") || r.MemberNames.Contains("TicketNumber")));
        }

        [Fact]
        public void Ticket_ValidData_PassesValidation()
        {
            // Arrange
            var ticket = new Ticket
            {
                PurchaseId = 1,
                QRCodeData = "QR123456789",
                TicketNumber = "TKT-001",
                IsUsed = false
            };

            // Act
            var context = new ValidationContext(ticket);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(ticket, context, results, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void Ticket_WhenRedeemed_RedeemedAtIsSet()
        {
            // Arrange
            var ticket = new Ticket
            {
                PurchaseId = 1,
                QRCodeData = "QR123",
                TicketNumber = "TKT-001"
            };

            // Act
            ticket.IsUsed = true;
            ticket.RedeemedAt = DateTime.Now;

            // Assert
            Assert.True(ticket.IsUsed);
            Assert.NotNull(ticket.RedeemedAt);
        }
    }
}
