using System;
using System.ComponentModel.DataAnnotations;
namespace Home_Sbdv.Models
{
    public class VisitorPassRequestViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Visitor name is required")]
        [Display(Name = "Visitor Name")]
        public string VisitorName { get; set; } = string.Empty;

        [Display(Name = "Contact Number")]
        public string VisitorContact { get; set; } = string.Empty;

        public string Purpose { get; set; } = string.Empty;

        [Required(ErrorMessage = "Visit date is required")]
        [Display(Name = "Visit Date")]
        [DataType(DataType.Date)]
        public DateTime VisitDate { get; set; } = DateTime.Today;

        public string Status { get; set; } = "Pending";

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public int RequestedByUserId { get; set; }

        public string RequestedByUserName { get; set; } = string.Empty;

        public int? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string AuditTrail { get; set; } = string.Empty;

        // Search/filter properties - not required for model validation
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Status")]
        public string? StatusFilter { get; set; }

        [Display(Name = "Date")]
        public DateTime? DateFilter { get; set; }
    }
}