using System.ComponentModel.DataAnnotations;
using COMP2138_ICE.Models;
using Xunit;

namespace COMP2138_ICE.Tests
{
    public class PurchaseModelTests
    {
        [Fact]
        public void Purchase_TicketQuantity_MustBeAtLeastOne()
        {
            // Arrange
            var purchase = new Purchase
            {
                EventId = 1,
                UserId = "user123",
                TicketQuantity = 0, // Invalid
                PurchaseDate = DateTime.Now,
                TotalAmount = 0
            };

            // Act
            var context = new ValidationContext(purchase);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(purchase, context, results, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("TicketQuantity"));
        }

        [Fact]
        public void Purchase_ValidData_PassesValidation()
        {
            // Arrange
            var purchase = new Purchase
            {
                EventId = 1,
                UserId = "user123",
                OrderNumber = "ORD-12345",
                TicketQuantity = 2,
                PurchaseDate = DateTime.Now,
                TotalAmount = 100.00m
            };

            // Act
            var context = new ValidationContext(purchase);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(purchase, context, results, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void Purchase_RequiredFields_AreValidated()
        {
            // Arrange
            var purchase = new Purchase
            {
                // Missing required fields - EventId defaults to 0, TicketQuantity defaults to 0
            };

            // Act
            var context = new ValidationContext(purchase);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(purchase, context, results, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("UserId"));
            Assert.Contains(results, r => r.MemberNames.Contains("TicketQuantity"));
        }
    }
}
