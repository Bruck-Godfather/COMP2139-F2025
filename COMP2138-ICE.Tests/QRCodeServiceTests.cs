using COMP2138_ICE.Services;
using Moq;
using Xunit;

namespace COMP2138_ICE.Tests
{
    public class QRCodeServiceTests
    {
        [Fact]
        public void GenerateQRCode_WithValidData_ReturnsNonEmptyByteArray()
        {
            // Arrange
            var qrService = new QRCodeService();
            var testData = "TICKET-12345";

            // Act
            var result = qrService.GenerateQRCode(testData);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GenerateQRCode_WithEmptyString_ReturnsNonEmptyByteArray()
        {
            // Arrange
            var qrService = new QRCodeService();
            var testData = "";

            // Act
            var result = qrService.GenerateQRCode(testData);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GenerateQRCode_WithDifferentData_ReturnsDifferentQRCodes()
        {
            // Arrange
            var qrService = new QRCodeService();
            var data1 = "TICKET-001";
            var data2 = "TICKET-002";

            // Act
            var qr1 = qrService.GenerateQRCode(data1);
            var qr2 = qrService.GenerateQRCode(data2);

            // Assert
            Assert.NotEqual(qr1, qr2);
        }

        [Fact]
        public void GenerateQRCode_WithSameData_ReturnsSameQRCode()
        {
            // Arrange
            var qrService = new QRCodeService();
            var data = "TICKET-123";

            // Act
            var qr1 = qrService.GenerateQRCode(data);
            var qr2 = qrService.GenerateQRCode(data);

            // Assert
            Assert.Equal(qr1, qr2);
        }
    }
}
