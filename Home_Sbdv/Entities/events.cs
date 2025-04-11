using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Home_Sbdv.Entities
{
    public class Event
    {
        [Key]
        [Column("event_id")]
        public int EventId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("event_name")]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [Column("description")]
        public string EventDescription { get; set; } = string.Empty;

        [Required]
        [Column("event_date")]
        public DateTime EventDate { get; set; }

        [Required]
        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Column("end_time")]
        public TimeSpan EndTime { get; set; }

        [Column("location")]
        [StringLength(255)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("last_updated")]
        public DateTime? LastUpdated { get; set; }

        // Navigation Property
        [ForeignKey("CreatedBy")]
        public virtual Users? User { get; set; }
    }
}