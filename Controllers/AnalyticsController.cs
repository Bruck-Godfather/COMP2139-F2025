using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP2138_ICE.Data;
using COMP2138_ICE.Models;

namespace COMP2138_ICE.Controllers
{
    [Authorize(Roles = "Admin,Organizer")]
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnalyticsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesByCategory()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var query = _context.Purchases
                .Include(p => p.Event)
                .ThenInclude(e => e.Category)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(p => p.Event.OrganizerId == user.Id);
            }

            var data = await query
                .GroupBy(p => p.Event.Category.Name)
                .Select(g => new { Category = g.Key, Count = g.Sum(p => p.TicketQuantity) })
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenueByMonth()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var query = _context.Purchases
                .Include(p => p.Event)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(p => p.Event.OrganizerId == user.Id);
            }

            var data = await query
                .GroupBy(p => new { p.PurchaseDate.Year, p.PurchaseDate.Month })
                .Select(g => new { 
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"), 
                    Revenue = g.Sum(p => p.TotalAmount) 
                })
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetTopEvents()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var query = _context.Events
                .Include(e => e.Purchases)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(e => e.OrganizerId == user.Id);
            }

            var data = await query
                .Select(e => new {
                    Title = e.Title,
                    TicketsSold = e.Purchases.Sum(p => p.TicketQuantity),
                    Revenue = e.Purchases.Sum(p => p.TotalAmount)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            return Json(data);
        }
        // GET: Analytics/EventStats/5
        public async Task<IActionResult> EventStats(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var @event = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Purchases)
                    .ThenInclude(p => p.Tickets)
                .Include(e => e.Ratings)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@event == null) return NotFound();

            // Verify ownership
            if (@event.OrganizerId != user.Id && !isAdmin)
            {
                return Forbid();
            }

            // Calculate stats
            var totalTicketsSold = @event.Purchases?.Sum(p => p.TicketQuantity) ?? 0;
            var totalRevenue = @event.Purchases?.Sum(p => p.TotalAmount) ?? 0;
            var totalAttended = @event.Purchases?
                .SelectMany(p => p.Tickets)
                .Count(t => t.IsUsed) ?? 0;
            
            var ratings = @event.Ratings ?? new List<Rating>();
            var averageRating = ratings.Any() ? ratings.Average(r => r.Stars) : 0;
            
            ViewBag.TotalTicketsSold = totalTicketsSold;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalAttended = totalAttended;
            ViewBag.AverageRating = averageRating;
            ViewBag.RatingCount = ratings.Count;

            return View(@event);
        }
    }
}
