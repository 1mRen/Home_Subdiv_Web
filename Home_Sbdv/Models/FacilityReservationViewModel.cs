using System.ComponentModel.DataAnnotations;
using Home_Sbdv.Entities;

namespace Home_Sbdv.Models
{
    public class FacilityReservationViewModel
    {
        public int ReservationId { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "Facility selection is required.")]
        public int FacilityId { get; set; }

        public string FacilityName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reservation date is required.")]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        public int CreatedBy { get; set; }

        public string CreatedByName { get; set; } = string.Empty;

        // Optional for use in views
        public virtual Users? User { get; set; }
        public virtual Facilities? Facility { get; set; }

        // Helper properties for staff/user logic
        public bool CanApprove(int currentUserId) => Status == "Pending" && UserId != currentUserId;
        public bool CanDisapprove(int currentUserId) => Status == "Pending" && UserId != currentUserId;
        public bool CanCancel(int currentUserId) => (Status == "Pending" || Status == "Approved") && UserId == currentUserId;
    }
}
