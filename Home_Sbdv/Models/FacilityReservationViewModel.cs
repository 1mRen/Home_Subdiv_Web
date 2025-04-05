using Home_Sbdv.Entities;
using System.ComponentModel.DataAnnotations;

public class FacilityReservationViewModel
{
    public int ReservationId { get; set; }

    public int UserId { get; set; }

    [Required]
    public int FacilityId { get; set; }

    public string FacilityName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime ReservationDate { get; set; }

    [Required]
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; }

    [Required]
    [DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; }

    [Required]
    public string Status { get; set; } = "Pending";

    public int CreatedBy { get; set; }

    public string CreatedByName { get; set; } = string.Empty;

    // Optional for use in views
    public virtual Users? User { get; set; }
    public virtual Facilities? Facility { get; set; }
}
