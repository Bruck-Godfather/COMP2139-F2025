namespace COMP2138_ICE.Models
{
    public class CartItem
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}
