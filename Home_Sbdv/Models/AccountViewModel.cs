using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Home_Sbdv.Models
{
    public class AccountViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required, EmailAddress, StringLength(100)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required, StringLength(25)]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        [Required, StringLength(255)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Required, StringLength(10)]
        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        [Required, StringLength(10)]
        [Display(Name = "Ownership Status")]
        public string? OwnershipStatus { get; set; }

        [Display(Name = "Profile Picture")]
        public string? ProfilePictureUrl { get; set; }

        [Display(Name = "Upload New Profile Picture")]
        public IFormFile? ProfilePictureFile { get; set; }
    }
} 