using System.ComponentModel.DataAnnotations;

namespace COMP2138_ICE.Models
{
    public class Event
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Event Title")]
        public string Title { get; set; }
        
        public string? Description { get; set; }
        public string? ImagePath { get; set; }

        [Required]
        public int CategoryId { get; set; }
        
        // Navigation property
        public Category? Category { get; set; }
        
        [Required]
        public string OrganizerId { get; set; }
        public ApplicationUser? Organizer { get; set; }

        [Required]
        [Display(Name = "Event Date & Time")]
        public DateTime DateTime { get; set; }

        [Display(Name = "Time Zone")]
        public string TimeZoneId { get; set; } = "UTC"; // Default to UTC
        
        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Ticket Price")]
        public decimal Price { get; set; }
        
        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Tickets Available")]
        public int TicketsAvailable { get; set; }

        public ICollection<Purchase>? Purchases { get; set; }
        public ICollection<Rating>? Ratings { get; set; }
    }
}
