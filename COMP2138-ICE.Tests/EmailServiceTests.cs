using COMP2138_ICE.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace COMP2138_ICE.Tests
{
    public class EmailServiceTests
    {
        [Fact]
        public async Task SendEmailAsync_WithValidConfiguration_DoesNotThrow()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            var mockEmailSection = new Mock<IConfigurationSection>();
            
            // Setup configuration values
            mockConfiguration.Setup(c => c.GetSection("EmailSettings")).Returns(mockEmailSection.Object);
            mockEmailSection.Setup(s => s["SenderName"]).Returns("Test Sender");
            mockEmailSection.Setup(s => s["SenderEmail"]).Returns("test@example.com");
            mockEmailSection.Setup(s => s["SenderPassword"]).Returns("password");
            mockEmailSection.Setup(s => s["SmtpServer"]).Returns("smtp.example.com");
            mockEmailSection.Setup(s => s["SmtpPort"]).Returns("587");

            var emailService = new EmailService(mockConfiguration.Object);

            // Act & Assert - Should not throw exception even if SMTP fails
            // The service catches exceptions internally
            await emailService.SendEmailAsync("recipient@example.com", "Test Subject", "Test Message");
            
            // If we reach here, the test passed (no exception thrown)
            Assert.True(true);
        }

        [Fact]
        public void EmailService_Constructor_AcceptsConfiguration()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();

            // Act
            var emailService = new EmailService(mockConfiguration.Object);

            // Assert
            Assert.NotNull(emailService);
        }
    }
}
