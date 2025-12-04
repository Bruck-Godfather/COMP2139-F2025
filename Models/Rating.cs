using System.ComponentModel.DataAnnotations;

namespace COMP2138_ICE.Models
{
    public class Rating
    {
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }
        public Event? Event { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required]
        public int PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }

        [Range(1, 5)]
        public int Stars { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
