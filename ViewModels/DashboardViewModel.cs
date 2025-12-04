using COMP2138_ICE.Models;

namespace COMP2138_ICE.ViewModels
{
    public class DashboardViewModel
    {
        public ApplicationUser User { get; set; }
        
        // For Attendees
        public COMP2138_ICE.Helpers.PaginatedList<Purchase> UpcomingTickets { get; set; }
        public COMP2138_ICE.Helpers.PaginatedList<Purchase> PurchaseHistory { get; set; }
        
        // For Organizers
        public COMP2138_ICE.Helpers.PaginatedList<Event> MyEvents { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
