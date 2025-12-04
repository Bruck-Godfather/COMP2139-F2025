using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP2138_ICE.Data;
using COMP2138_ICE.Models;

namespace COMP2138_ICE.Controllers
{
    [Authorize]
    public class RatingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RatingController> _logger;

        public RatingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<RatingController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Check if user can rate a purchase
        [HttpGet]
        public async Task<IActionResult> CanRate(int purchaseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { canRate = false, reason = "User not found" });

            var purchase = await _context.Purchases
                .Include(p => p.Event)
                .Include(p => p.Tickets)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == purchaseId);

            if (purchase == null)
                return Json(new { canRate = false, reason = "Purchase not found" });

            // Check 1: Purchase must belong to the user
            if (purchase.UserId != user.Id)
                return Json(new { canRate = false, reason = "Not your purchase" });

            // Check 2: Event date must have passed (timezone-aware)
            if (purchase.Event == null)
                return Json(new { canRate = false, reason = "Event not found" });

            // Convert to event's timezone for date comparison
            TimeZoneInfo eventTimeZone;
            try
            {
                eventTimeZone = TimeZoneInfo.FindSystemTimeZoneById(purchase.Event.TimeZoneId ?? "UTC");
            }
            catch
            {
                eventTimeZone = TimeZoneInfo.Utc;
            }

            // Event DateTime is stored as local time in the event's timezone
            var eventLocalTime = purchase.Event.DateTime;
            var nowInEventTimeZone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, eventTimeZone);

            if (nowInEventTimeZone.Date < eventLocalTime.Date)
                return Json(new { canRate = false, reason = $"Event hasn't occurred yet. Wait until {eventLocalTime:d}" });

            // Check 3: At least one ticket must be scanned (attended)
            var hasAttended = purchase.Tickets?.Any(t => t.IsUsed) ?? false;
            if (!hasAttended)
                return Json(new { canRate = false, reason = "You must attend the event to rate it. No tickets have been scanned." });

            // Check 4: User hasn't already rated this purchase
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.PurchaseId == purchaseId && r.UserId == user.Id);

            if (existingRating != null)
                return Json(new { canRate = false, reason = "You have already rated this event", hasRating = true });

            return Json(new { canRate = true });
        }

        // POST: Submit a rating
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRating(int purchaseId, int stars, string? comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "User not found" });

            // Validate stars
            if (stars < 1 || stars > 5)
                return Json(new { success = false, message = "Rating must be between 1 and 5 stars" });

            // Re-check eligibility
            var purchase = await _context.Purchases
                .Include(p => p.Event)
                .Include(p => p.Tickets)
                .FirstOrDefaultAsync(p => p.Id == purchaseId);

            if (purchase == null)
                return Json(new { success = false, message = "Purchase not found" });

            if (purchase.UserId != user.Id)
                return Json(new { success = false, message = "Unauthorized" });

            // Timezone-aware date check
            if (purchase.Event == null)
                return Json(new { success = false, message = "Event not found" });

            TimeZoneInfo eventTimeZone;
            try
            {
                eventTimeZone = TimeZoneInfo.FindSystemTimeZoneById(purchase.Event.TimeZoneId ?? "UTC");
            }
            catch
            {
                eventTimeZone = TimeZoneInfo.Utc;
            }

            // Event DateTime is stored as local time in the event's timezone
            var eventLocalTime = purchase.Event.DateTime;
            var nowInEventTimeZone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, eventTimeZone);

            if (nowInEventTimeZone.Date < eventLocalTime.Date)
                return Json(new { success = false, message = "Event hasn't occurred yet" });

            var hasAttended = purchase.Tickets?.Any(t => t.IsUsed) ?? false;
            if (!hasAttended)
                return Json(new { success = false, message = "You must attend the event to rate it" });

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.PurchaseId == purchaseId && r.UserId == user.Id);

            if (existingRating != null)
                return Json(new { success = false, message = "You have already rated this event" });

            // Create rating
            var rating = new Rating
            {
                EventId = purchase.EventId,
                UserId = user.Id,
                PurchaseId = purchaseId,
                Stars = stars,
                Comment = comment?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} rated Event {EventId} with {Stars} stars", user.Id, purchase.EventId, stars);

            return Json(new { success = true, message = "Thank you for your rating!" });
        }

        // GET: Get rating for a purchase (if authorized)
        [HttpGet]
        public async Task<IActionResult> GetRating(int purchaseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { hasRating = false });

            var purchase = await _context.Purchases
                .Include(p => p.Event)
                .FirstOrDefaultAsync(p => p.Id == purchaseId);

            if (purchase == null)
                return Json(new { hasRating = false });

            var rating = await _context.Ratings
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.PurchaseId == purchaseId);

            if (rating == null)
                return Json(new { hasRating = false });

            // Check visibility: only show to event creator, admin, or the user who rated
            var isEventCreator = purchase.Event?.OrganizerId == user.Id;
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isAuthor = rating.UserId == user.Id;

            if (!isEventCreator && !isAdmin && !isAuthor)
                return Json(new { hasRating = false, message = "Not authorized to view this rating" });

            return Json(new
            {
                hasRating = true,
                stars = rating.Stars,
                comment = rating.Comment,
                createdAt = rating.CreatedAt.ToString("g"),
                authorName = isEventCreator || isAdmin ? rating.User?.UserName : "You"
            });
        }
    }
}
