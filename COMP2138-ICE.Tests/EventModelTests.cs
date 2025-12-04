using System.ComponentModel.DataAnnotations;
using COMP2138_ICE.Models;
using Xunit;

namespace COMP2138_ICE.Tests
{
    public class EventModelTests
    {
        [Fact]
        public void Event_Title_IsRequired()
        {
            // Arrange
            var eventModel = new Event
            {
                Title = null!, // Intentionally null
                CategoryId = 1,
                OrganizerId = "user123",
                DateTime = DateTime.Now.AddDays(7),
                Price = 50.00m,
                TicketsAvailable = 100
            };

            // Act
            var context = new ValidationContext(eventModel);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(eventModel, context, results, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Title"));
        }

        [Fact]
        public void Event_Price_MustBeNonNegative()
        {
            // Arrange
            var eventModel = new Event
            {
                Title = "Test Event",
                CategoryId = 1,
                OrganizerId = "user123",
                DateTime = DateTime.Now.AddDays(7),
                Price = -10.00m, // Invalid negative price
                TicketsAvailable = 100
            };

            // Act
            var context = new ValidationContext(eventModel);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(eventModel, context, results, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Price"));
        }

        [Fact]
        public void Event_TicketsAvailable_MustBeNonNegative()
        {
            // Arrange
            var eventModel = new Event
            {
                Title = "Test Event",
                CategoryId = 1,
                OrganizerId = "user123",
                DateTime = DateTime.Now.AddDays(7),
                Price = 50.00m,
                TicketsAvailable = -5 // Invalid negative tickets
            };

            // Act
            var context = new ValidationContext(eventModel);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(eventModel, context, results, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("TicketsAvailable"));
        }

        [Fact]
        public void Event_ValidData_PassesValidation()
        {
            // Arrange
            var eventModel = new Event
            {
                Title = "Test Event",
                Description = "A test event description",
                CategoryId = 1,
                OrganizerId = "user123",
                DateTime = DateTime.Now.AddDays(7),
                TimeZoneId = "UTC",
                Price = 50.00m,
                TicketsAvailable = 100
            };

            // Act
            var context = new ValidationContext(eventModel);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(eventModel, context, results, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void Event_DefaultTimeZone_IsUTC()
        {
            // Arrange & Act
            var eventModel = new Event();

            // Assert
            Assert.Equal("UTC", eventModel.TimeZoneId);
        }
    }
}
