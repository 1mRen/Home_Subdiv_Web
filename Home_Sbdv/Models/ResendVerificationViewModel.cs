using System.ComponentModel.DataAnnotations;

namespace Home_Sbdv.Models
{
    public class ResendVerificationViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email address")]
        public string Email { get; set; }
    }
}