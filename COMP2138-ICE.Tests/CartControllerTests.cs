using COMP2138_ICE.Controllers;
using COMP2138_ICE.Data;
using COMP2138_ICE.Models;
using COMP2138_ICE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace COMP2138_ICE.Tests
{
    public class CartControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            return mockUserManager;
        }

        private ISession CreateMockSession()
        {
            var mockSession = new MockHttpSession();
            return mockSession;
        }

        [Fact]
        public void AddToCart_WithValidEvent_ReturnsSuccess()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var mockUserManager = GetMockUserManager();
            var mockQRService = new Mock<IQRCodeService>();
            var mockLogger = new Mock<ILogger<CartController>>();

            var category = new Category { Id = 1, Name = "Music" };
            context.Categories.Add(category);
            context.SaveChanges();

            var testEvent = new Event
            {
                Id = 1,
                Title = "Test Concert",
                CategoryId = 1,
                OrganizerId = "user1",
                DateTime = DateTime.Now.AddDays(1),
                Price = 50,
                TicketsAvailable = 100
            };
            context.Events.Add(testEvent);
            context.SaveChanges();

            var controller = new CartController(context, mockUserManager.Object, mockQRService.Object, mockLogger.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Session = CreateMockSession();
            
            // Mock authenticated user
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "testuser"),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "user1")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(identity);
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.AddToCart(1, 2);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        [Fact]
        public void AddToCart_WithInvalidEventId_ReturnsNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var mockUserManager = GetMockUserManager();
            var mockQRService = new Mock<IQRCodeService>();
            var mockLogger = new Mock<ILogger<CartController>>();

            var controller = new CartController(context, mockUserManager.Object, mockQRService.Object, mockLogger.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Session = CreateMockSession();
            
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "testuser")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(identity);
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.AddToCart(999, 2);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var mockUserManager = GetMockUserManager();
            var mockQRService = new Mock<IQRCodeService>();
            var mockLogger = new Mock<ILogger<CartController>>();

            var controller = new CartController(context, mockUserManager.Object, mockQRService.Object, mockLogger.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Session = CreateMockSession();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }
    }

    // Mock Session for testing
    public class MockHttpSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new();

        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear() => _sessionStorage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _sessionStorage.Remove(key);

        public void Set(string key, byte[] value) => _sessionStorage[key] = value;

        public bool TryGetValue(string key, out byte[] value) 
        {
            if (_sessionStorage.TryGetValue(key, out var val))
            {
                value = val;
                return true;
            }
            value = null!;
            return false;
        }
    }
}
