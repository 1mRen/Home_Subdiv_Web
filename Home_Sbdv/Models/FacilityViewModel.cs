using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Home_Sbdv.Models
{
    public class FacilityViewModel
    {
        [Key]
        [Column("facility_id")]
        public int FacilityId { get; set; }

        [Required(ErrorMessage = "Facility name is required.")]
        [StringLength(255, ErrorMessage = "Facility name must not exceed 255 characters.")]
        [Column("facility_name")]
        [Display(Name = "Facility Name")]
        public required string FacilityName { get; set; } = String.Empty;

        [Required(ErrorMessage = "Facility description is required.")]
        [Column("Description")]
        [Display(Name = "Description")]
        public required string Description { get; set; } = String.Empty;

        [Display(Name = "Image")]
        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Facility's location is required")]
        [Column("location")]
        [StringLength(255, ErrorMessage = "Location must not exceed 255 characters.")]
        public string Location { get; set; } = String.Empty;

        [Display(Name = "Capacity")]
        [Column("capacity")]
        public int? Capacity { get; set; }

        [Required(ErrorMessage = "Availability status is required.")]
        [Column("availability_status")]
        [Display(Name = "Availability Status")]
        public string AvailabilityStatus { get; set; } = "Available";

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        // Helper property for image display
        [NotMapped]
        public string DisplayImageUrl => !string.IsNullOrEmpty(ImageUrl) ? ImageUrl : "~/images/default-facility.jpg";

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}
