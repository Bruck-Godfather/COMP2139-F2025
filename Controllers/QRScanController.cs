using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP2138_ICE.Data;
using COMP2138_ICE.Models;

namespace COMP2138_ICE.Controllers
{
    [Authorize(Roles = "Admin,Organizer")]
    public class QRScanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QRScanController> _logger;

        public QRScanController(ApplicationDbContext context, ILogger<QRScanController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: QRScan
        public IActionResult Index()
        {
            return View();
        }

        // POST: QRScan/ScanTicket
        [HttpPost]
        public async Task<IActionResult> ScanTicket(string ticketNumber)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber))
            {
                return Json(new { success = false, message = "Ticket number is required." });
            }

            var ticket = await _context.Tickets
                .Include(t => t.Purchase)
                    .ThenInclude(p => p.Event)
                .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber);

            if (ticket == null)
            {
                _logger.LogWarning("Invalid ticket scan attempt: {TicketNumber}", ticketNumber);
                return Json(new { success = false, message = "Invalid ticket number." });
            }

            // Check if ticket already used
            if (ticket.IsUsed)
            {
                _logger.LogInformation("Duplicate scan attempt for ticket: {TicketNumber}", ticketNumber);
                return Json(new 
                { 
                    success = false, 
                    message = $"Ticket already scanned on {ticket.RedeemedAt?.ToString("g")}.",
                    alreadyUsed = true
                });
            }

            // Validate event date - can scan on or after event date (timezone-aware)
            var evt = ticket.Purchase?.Event;
            if (evt == null)
            {
                _logger.LogError("Event not found for ticket: {TicketNumber}", ticketNumber);
                return Json(new { success = false, message = "Event information not found." });
            }

            // Convert UTC event time to event's local timezone
            TimeZoneInfo eventTimeZone;
            try
            {
                eventTimeZone = TimeZoneInfo.FindSystemTimeZoneById(evt.TimeZoneId ?? "UTC");
            }
            catch
            {
                eventTimeZone = TimeZoneInfo.Utc;
                _logger.LogWarning("Invalid timezone {TimeZoneId} for event {EventId}, using UTC", evt.TimeZoneId, evt.Id);
            }

            // Event DateTime is stored as local time in the event's timezone
            var eventLocalTime = evt.DateTime;
            var nowInEventTimeZone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, eventTimeZone);

            // Compare dates only (ignore time)
            if (nowInEventTimeZone.Date < eventLocalTime.Date)
            {
                _logger.LogWarning("Early scan attempt for ticket: {TicketNumber}. Event date: {EventDate}, Current date: {CurrentDate}", 
                    ticketNumber, eventLocalTime.Date, nowInEventTimeZone.Date);
                return Json(new 
                { 
                    success = false, 
                    message = $"This ticket cannot be scanned until {eventLocalTime:d}. Event has not started yet.",
                    tooEarly = true
                });
            }

            // Mark ticket as used
            ticket.IsUsed = true;
            ticket.RedeemedAt = DateTime.UtcNow;
            _context.Update(ticket);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket scanned successfully: {TicketNumber} for Event: {EventTitle}", 
                ticketNumber, evt.Title);

            return Json(new 
            { 
                success = true, 
                message = "Ticket scanned successfully!",
                eventTitle = evt.Title,
                attendeeName = ticket.Purchase?.User?.UserName,
                scannedAt = DateTime.UtcNow.ToString("g")
            });
        }
    }
}
