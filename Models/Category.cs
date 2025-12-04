using System.ComponentModel.DataAnnotations;

namespace COMP2138_ICE.Models
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        // Navigation property
        public ICollection<Event>? Events { get; set; }
    }
}
