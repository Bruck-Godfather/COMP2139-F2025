using System.ComponentModel.DataAnnotations;

namespace COMP2138_ICE.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        
        [Required]
        public int EventId { get; set; }
        
        // Navigation property
        public Event? Event { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string? OrderNumber { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "Number of Tickets")]
        public int TicketQuantity { get; set; }
        
        [Required]
        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; }
        
        [Required]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        public ICollection<Ticket>? Tickets { get; set; }
        public ICollection<Rating>? Ratings { get; set; }
    }
}
