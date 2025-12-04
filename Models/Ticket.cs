using System.ComponentModel.DataAnnotations;

namespace COMP2138_ICE.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        public int PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }

        [Required]
        public string QRCodeData { get; set; } = string.Empty;

        [Required]
        public string TicketNumber { get; set; } = string.Empty;

        public bool IsUsed { get; set; } = false;
        public DateTime? RedeemedAt { get; set; }
    }
}
