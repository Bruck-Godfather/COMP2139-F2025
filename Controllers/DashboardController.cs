using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP2138_ICE.Data;
using COMP2138_ICE.Models;
using COMP2138_ICE.ViewModels;
using COMP2138_ICE.Services;

namespace COMP2138_ICE.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IPdfService _pdfService;

        public DashboardController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IPdfService pdfService)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _pdfService = pdfService;
        }

        public async Task<IActionResult> Index(int? ticketPage, int? historyPage, int? eventsPage, int? usersPage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Admin Dashboard Logic
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                int pageSize = 10;
                var allUsersQuery = _userManager.Users
                    .Where(u => u.Id != user.Id) // Exclude current admin
                    .OrderBy(u => u.Email);

                var paginatedUsers = await COMP2138_ICE.Helpers.PaginatedList<ApplicationUser>.CreateAsync(
                    allUsersQuery.AsNoTracking(), usersPage ?? 1, pageSize);

                var userViewModels = new List<UserDetailViewModel>();

                foreach (var u in paginatedUsers)
                {
                    var roles = await _userManager.GetRolesAsync(u);
                    
                    var purchases = await _context.Purchases
                        .Include(p => p.Event)
                        .Where(p => p.UserId == u.Id)
                        .OrderByDescending(p => p.PurchaseDate)
                        .ToListAsync();

                    var ratings = await _context.Ratings
                        .Include(r => r.Event)
                        .Where(r => r.UserId == u.Id)
                        .OrderByDescending(r => r.CreatedAt)
                        .ToListAsync();

                    userViewModels.Add(new UserDetailViewModel
                    {
                        User = u,
                        Roles = roles,
                        Purchases = purchases,
                        Ratings = ratings
                    });
                }

                var adminViewModel = new AdminDashboardViewModel
                {
                    Users = new COMP2138_ICE.Helpers.PaginatedList<UserDetailViewModel>(
                        userViewModels, paginatedUsers.TotalPages, paginatedUsers.PageIndex, paginatedUsers.TotalPages) 
                        // Note: Re-wrapping list to keep pagination metadata. 
                        // Ideally PaginatedList should be generic enough or we map differently, 
                        // but for now we construct it manually or use the source's metadata.
                        // Let's adjust: We'll just pass the list and manual props if needed, 
                        // or better, let's just use the paginatedUsers metadata.
                };
                
                // Hack: Re-creating PaginatedList from list + metadata is not directly supported by the helper's CreateAsync.
                // Let's just assign the list and copy properties if we can, or modify ViewModel.
                // Simpler approach: Pass the PaginatedList<ApplicationUser> to view and fetch details there? 
                // No, N+1 problem in view.
                // Better: Create a new PaginatedList constructor or property.
                // For this task, I'll just create a new PaginatedList instance with the mapped data.
                
                adminViewModel.Users = new COMP2138_ICE.Helpers.PaginatedList<UserDetailViewModel>(
                    userViewModels, paginatedUsers.TotalPages, paginatedUsers.PageIndex, pageSize); 
                    // Wait, the constructor takes (items, count, pageIndex, pageSize). 
                    // We need total count.
                
                 adminViewModel.Users = new COMP2138_ICE.Helpers.PaginatedList<UserDetailViewModel>(
                    userViewModels, allUsersQuery.Count(), usersPage ?? 1, pageSize);

                return View("AdminDashboard", adminViewModel);
            }

            // Standard User/Organizer Dashboard Logic
            var viewModel = new DashboardViewModel
            {
                User = user
            };

            int defaultPageSize = 15;

            // Load Purchases with Tickets and Events
            var purchasesQuery = _context.Purchases
                .Include(p => p.Event)
                .Include(p => p.Tickets)
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.PurchaseDate);

            var now = DateTime.UtcNow;

            // Upcoming Tickets
            var upcomingQuery = purchasesQuery
                .Where(p => p.Event != null && p.Event.DateTime > now);
            
            viewModel.UpcomingTickets = await COMP2138_ICE.Helpers.PaginatedList<Purchase>.CreateAsync(
                upcomingQuery.AsNoTracking(), ticketPage ?? 1, defaultPageSize);

            // Purchase History (All)
            viewModel.PurchaseHistory = await COMP2138_ICE.Helpers.PaginatedList<Purchase>.CreateAsync(
                purchasesQuery.AsNoTracking(), historyPage ?? 1, defaultPageSize);

            // If Organizer, load their events
            if (await _userManager.IsInRoleAsync(user, "Organizer"))
            {
                var myEventsQuery = _context.Events
                    .Include(e => e.Purchases)
                    .Where(e => e.OrganizerId == user.Id)
                    .OrderByDescending(e => e.DateTime);

                viewModel.MyEvents = await COMP2138_ICE.Helpers.PaginatedList<Event>.CreateAsync(
                    myEventsQuery.AsNoTracking(), eventsPage ?? 1, defaultPageSize);

                // Calculate total revenue (separate query for total)
                viewModel.TotalRevenue = await _context.Events
                    .Where(e => e.OrganizerId == user.Id)
                    .SelectMany(e => e.Purchases)
                    .SumAsync(p => p.TotalAmount);
            }

            ViewData["TicketPage"] = ticketPage ?? 1;
            ViewData["HistoryPage"] = historyPage ?? 1;
            ViewData["EventsPage"] = eventsPage ?? 1;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ApplicationUser model, IFormFile? profilePicture)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.DateOfBirth = model.DateOfBirth;
            user.PhoneNumber = model.PhoneNumber;
            // Update other fields if necessary

            if (profilePicture != null && profilePicture.Length > 0)
            {
                // Ensure directory exists
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + profilePicture.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(fileStream);
                }

                // Delete old picture if exists
                if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfilePicturePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                user.ProfilePicturePath = "/uploads/profiles/" + uniqueFileName;
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DownloadTicket(int ticketId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Purchase)
                    .ThenInclude(p => p.Event)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            
            // Verify ownership
            if (ticket.Purchase.UserId != user.Id && !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Forbid();
            }

            var pdfBytes = _pdfService.GenerateTicketPdf(ticket, ticket.Purchase.Event, user);
            return File(pdfBytes, "application/pdf", $"Ticket-{ticket.TicketNumber}.pdf");
        }
    }
}
