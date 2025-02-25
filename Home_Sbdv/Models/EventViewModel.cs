using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Home_Sbdv.Models
{
    public class EventViewModel
    {
        [Key]
        [Column("event_id")]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Event name is required.")]
        [StringLength(255, ErrorMessage = "Event name must not exceed 255 characters.")]
        [Column("event_name")]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event description is required.")]
        [Column("description")]
        public string EventDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required.")]
        [Column("event_date")]
        [DataType(DataType.DateTime, ErrorMessage = "Invalid date format.")]
        public DateTime EventDate { get; set; }

        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("last_updated")]
        public DateTime? LastUpdated { get; set; }

        public string? CreatedByName { get; set; }
    }
}
