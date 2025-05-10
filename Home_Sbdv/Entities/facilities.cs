using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Home_Sbdv.Entities
{
    public class Facilities
    {
        [Key]
        [Column("facility_id")]
        public int FacilityId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("facility_name")]
        public string FacilityName { get; set; } = string.Empty;

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("image_url")]
        [StringLength(255)]
        public string? ImageUrl { get; set; }

        [Column("location")]
        [StringLength(255)]
        public string Location { get; set; } = string.Empty;

        [Column("capacity")]
        public int? Capacity { get; set; }

        [Column("availability_status")]
        [StringLength(20)]
        public string AvailabilityStatus { get; set; } = "Available";

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
