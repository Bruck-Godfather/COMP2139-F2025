using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP2138_ICE.Data;
using COMP2138_ICE.Models;
using COMP2138_ICE.Services;
using System.Text.Json;

namespace COMP2138_ICE.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IQRCodeService _qrCodeService;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            IQRCodeService qrCodeService,
            ILogger<CartController> logger)
        {
            _context = context;
            _userManager = userManager;
            _qrCodeService = qrCodeService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int eventId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.EventId == eventId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
                _logger.LogInformation("Item removed from cart: EventId {EventId}", eventId);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            if (User.IsInRole("Admin")) return Forbid();

            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction(nameof(Index));

            var user = await _userManager.GetUserAsync(User);
            
            foreach (var item in cart)
            {
                var evt = await _context.Events.FindAsync(item.EventId);
                if (evt == null || evt.TicketsAvailable < item.Quantity)
                {
                    _logger.LogWarning("Checkout failed: Event {EventId} not found or insufficient stock.", item.EventId);
                    // Handle error: Event not found or not enough tickets
                    continue; 
                }

                // Create Purchase
                var purchase = new Purchase
                {
                    UserId = user.Id,
                    EventId = item.EventId,
                    TicketQuantity = item.Quantity,
                    PurchaseDate = DateTime.UtcNow,
                    TotalAmount = evt.Price * item.Quantity,
                    OrderNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
                };

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync(); // Save to get PurchaseId

                _logger.LogInformation("Purchase successful: Order {OrderNumber} by User {Email} for Event {EventId}", purchase.OrderNumber, user.Email, item.EventId);

                // Create Tickets
                for (int i = 0; i < item.Quantity; i++)
                {
                    var ticketNumber = $"{purchase.OrderNumber}-{i + 1}";
                    var qrData = _qrCodeService.GenerateQRCode(ticketNumber);
                    
                    var ticket = new Ticket
                    {
                        PurchaseId = purchase.Id,
                        TicketNumber = ticketNumber,
                        QRCodeData = Convert.ToBase64String(qrData),
                        IsUsed = false
                    };
                    _context.Tickets.Add(ticket);
                }

                // Update Stock
                evt.TicketsAvailable -= item.Quantity;
                _context.Update(evt);
            }

            await _context.SaveChangesAsync();

            // Clear Cart
            HttpContext.Session.Remove("Cart");

            return RedirectToAction(nameof(OrderConfirmation));
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }

        // AJAX: Add to Cart
        [HttpPost]
        [AllowAnonymous]
        public IActionResult AddToCart(int eventId, int quantity)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, requiresLogin = true });
            }

            if (User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "Admins cannot purchase tickets." });
            }

            var evt = _context.Events.Find(eventId);
            if (evt == null) return NotFound();
            
            if (evt.DateTime < DateTime.Now)
            {
                return BadRequest("Cannot purchase tickets for past events.");
            }

            if (evt.TicketsAvailable < quantity)
            {
                return BadRequest("Not enough tickets available.");
            }

            // Get Cart from Session
            var cart = GetCart();

            // Add or Update Item
            var item = cart.FirstOrDefault(i => i.EventId == eventId);
            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    EventId = evt.Id,
                    EventTitle = evt.Title,
                    Price = evt.Price,
                    Quantity = quantity
                });
            }

            // Save Cart
            SaveCart(cart);

            return Json(new { success = true, message = "Added to cart", cartCount = cart.Sum(i => i.Quantity) });
        }

        // AJAX: Update Cart Quantity
        [HttpPost]
        public IActionResult UpdateQuantity(int eventId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.EventId == eventId);
            
            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    // Check stock
                    var evt = _context.Events.Find(eventId);
                    if (evt != null && evt.TicketsAvailable >= quantity)
                    {
                        item.Quantity = quantity;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Not enough stock" });
                    }
                }
                SaveCart(cart);
            }

            // Calculate totals
            var cartCount = cart.Sum(i => i.Quantity);
            var cartTotal = cart.Sum(i => i.Quantity * i.Price);
            var itemTotal = item != null ? item.Quantity * item.Price : 0;

            return Json(new { success = true, cartCount = cartCount, cartTotal = cartTotal, itemTotal = itemTotal });
        }

        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            return cartJson == null ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartJson);
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
        }
    }
}
