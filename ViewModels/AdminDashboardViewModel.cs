using COMP2138_ICE.Helpers;

namespace COMP2138_ICE.ViewModels
{
    public class AdminDashboardViewModel
    {
        public PaginatedList<UserDetailViewModel> Users { get; set; }
    }
}
