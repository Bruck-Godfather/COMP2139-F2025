using COMP2138_ICE.Models;

namespace COMP2138_ICE.ViewModels
{
    public class UserDetailViewModel
    {
        public ApplicationUser User { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public List<Purchase> Purchases { get; set; } = new List<Purchase>();
        public List<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
