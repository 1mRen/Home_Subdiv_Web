using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


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
        public required string FacilityName { get; set; } = String.Empty;

        [Required(ErrorMessage = "Facility description is required.")]
        [Column("Description")]
        public required string Description { get; set; } = String.Empty;

        [Required(ErrorMessage = "Facility's location is required")]
        [Column("location")]
        public string Location { get; set; } = String.Empty;

        [Required(ErrorMessage = "Availability status is required.")]
        [Column("availability_status")]
        public string AvailabilityStatus { get; set; } = String.Empty;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
