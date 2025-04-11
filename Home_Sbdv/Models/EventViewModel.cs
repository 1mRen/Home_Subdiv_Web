using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event description is required.")]
        [Column("description")]
        [Display(Name = "Description")]
        public string EventDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required.")]
        [Column("event_date")]
        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        [Column("start_time")]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [Column("end_time")]
        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; }

        [StringLength(255, ErrorMessage = "Location must not exceed 255 characters.")]
        [Column("location")]
        public string Location { get; set; } = string.Empty;

        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("last_updated")]
        public DateTime? LastUpdated { get; set; }

        public string? CreatedByName { get; set; }
    }
}