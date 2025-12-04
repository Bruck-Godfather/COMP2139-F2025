using System.ComponentModel.DataAnnotations;

namespace COMP2138_ICE.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
