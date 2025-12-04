using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace COMP2138_ICE.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        // PhoneNumber is inherited from IdentityUser, but we can add validation here if needed
        // public override string? PhoneNumber { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Profile Picture URL")]
        public string? ProfilePictureUrl { get; set; }

        [Display(Name = "Profile Picture")]
        public string? ProfilePicturePath { get; set; } // Kept for backward compatibility

        public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : (Email ?? "User");

        // Navigation properties
        public ICollection<Event>? OrganizedEvents { get; set; }
        public ICollection<Purchase>? Purchases { get; set; }
        public ICollection<Rating>? Ratings { get; set; }
    }
}
