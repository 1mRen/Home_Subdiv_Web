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
        public required string FacilityName { get; set; } = String.Empty;
        
        [Required]
        [Column("Description")]
        public required string Description {  get; set; } = String.Empty;
        
        [Required]
        [Column("location")]
        public string Location { get; set; } = String.Empty;
        
        [Required]
        [Column("availability_status")]
        public string AvailabilityStatus {  get; set; } = String.Empty;
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
