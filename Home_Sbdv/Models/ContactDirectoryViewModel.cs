using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Home_Sbdv.Models
{
    public class ContactDirectoryViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Department/Role")]
        public string? Department { get; set; }

        [StringLength(50)]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Photo/Logo")]
        public string? PhotoUrl { get; set; }

        [Display(Name = "Upload Photo/Logo")]
        public IFormFile? PhotoFile { get; set; }
    }
} 