using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COMP2138_ICE.Data;
using COMP2138_ICE.Models;

namespace COMP2138_ICE.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Event
        public async Task<IActionResult> Index(string searchString, int? categoryId, int? pageNumber, bool showPast = false)
        {
            var events = _context.Events.Include(e => e.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                events = events.Where(e => e.Title.ToLower().Contains(searchString.ToLower()));
            }

            if (categoryId.HasValue)
            {
                events = events.Where(e => e.CategoryId == categoryId);
            }

            // Filter by Date
            if (showPast)
            {
                events = events.Where(e => e.DateTime < DateTime.Now);
            }
            else
            {
                events = events.Where(e => e.DateTime >= DateTime.Now);
            }

            // Order by date (soonest first)
            events = events.OrderBy(e => e.DateTime);

            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryId;
            ViewData["ShowPast"] = showPast;
            
            int pageSize = 15; // Reverting to 15 as per original requirement
            return View(await COMP2138_ICE.Helpers.PaginatedList<Event>.CreateAsync(events.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // AJAX: Search Events
        [HttpPost]
        public async Task<IActionResult> SearchEvents(string searchString, int? categoryId, int? pageNumber, bool showPast = false)
        {
            var events = _context.Events.Include(e => e.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                events = events.Where(e => e.Title.ToLower().Contains(searchString.ToLower()));
            }

            if (categoryId.HasValue)
            {
                events = events.Where(e => e.CategoryId == categoryId);
            }

            // Filter by Date
            if (showPast)
            {
                events = events.Where(e => e.DateTime < DateTime.Now);
            }
            else
            {
                events = events.Where(e => e.DateTime >= DateTime.Now);
            }

            // Order by date (soonest first)
            events = events.OrderBy(e => e.DateTime);

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryId;
            ViewData["ShowPast"] = showPast;

            int pageSize = 15;
            return PartialView("_EventPartial", await COMP2138_ICE.Helpers.PaginatedList<Event>.CreateAsync(events.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Event/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // GET: Event/Create
        [Authorize(Roles = "Admin,Organizer")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,CategoryId,DateTime,Price,TicketsAvailable,TimeZoneId")] Event @event, IFormFile? imageFile)
        {
            ModelState.Remove("OrganizerId");
            ModelState.Remove("Organizer"); // Also remove navigation property if present
            if (ModelState.IsValid)
            {
                // Sanitize Description
                var sanitizer = new Ganss.Xss.HtmlSanitizer();
                @event.Description = sanitizer.Sanitize(@event.Description);

                var user = await _userManager.GetUserAsync(User);
                @event.OrganizerId = user.Id;

                // Handle Image Upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/events");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    @event.ImagePath = "/uploads/events/" + uniqueFileName;
                }

                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", @event.CategoryId);
            return View(@event);
        }

        // GET: Event/Edit/5
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();

            // Verify ownership
            var user = await _userManager.GetUserAsync(User);
            if (@event.OrganizerId != user.Id && !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Forbid();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", @event.CategoryId);
            return View(@event);
        }

        // POST: Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,CategoryId,DateTime,Price,TicketsAvailable,OrganizerId,ImagePath,TimeZoneId")] Event @event, IFormFile? imageFile)
        {
            if (id != @event.Id) return NotFound();

            // Verify ownership again
            var user = await _userManager.GetUserAsync(User);
            if (@event.OrganizerId != user.Id && !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Sanitize Description
                    var sanitizer = new Ganss.Xss.HtmlSanitizer();
                    @event.Description = sanitizer.Sanitize(@event.Description);

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete Old Image
                        if (!string.IsNullOrEmpty(@event.ImagePath))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", @event.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/events");
                        Directory.CreateDirectory(uploadsFolder);
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }
                        @event.ImagePath = "/uploads/events/" + uniqueFileName;
                    }

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", @event.CategoryId);
            return View(@event);
        }

        // GET: Event/Delete/5
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@event == null) return NotFound();

            // Verify ownership
            var user = await _userManager.GetUserAsync(User);
            if (@event.OrganizerId != user.Id && !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Forbid();
            }

            return View(@event);
        }

        // POST: Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                // Verify ownership
                var user = await _userManager.GetUserAsync(User);
                if (@event.OrganizerId != user.Id && !await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return Forbid();
                }

                // Delete Image
                if (!string.IsNullOrEmpty(@event.ImagePath))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", @event.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
