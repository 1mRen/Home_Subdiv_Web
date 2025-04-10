using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Home_Sbdv.Entities
{
    public class Event // ✅ Rename from Events to Event
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
        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("last_updated")]
        public DateTime? LastUpdated { get; set; }

        //  Navigation Property (Ensure it's set up correctly)
        [ForeignKey("CreatedBy")]
        public virtual Users? User { get; set; }
    }
}
