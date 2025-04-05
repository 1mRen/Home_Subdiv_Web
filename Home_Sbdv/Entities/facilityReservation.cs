using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Home_Sbdv.Entities
{
    public class FacilityReservation
    {  
            [Key]
            [Column("reservation_id")]
            public int ReservationId { get; set; }

            [Required]
            [ForeignKey("User")]
            [Column("user_id")]
            public int UserId { get; set; }

            [Required]
            [ForeignKey("Facility")]
            [Column("facility_id")]
            public int FacilityId { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Column("reservation_Date")]
            public DateTime ReservationDate { get; set; }

            [Required]
            [DataType(DataType.Time)]
            [Column("start_time")]
            public TimeSpan StartTime { get; set; }

            [Required]
            [DataType(DataType.Time)]
            [Column("end_time")]
            public TimeSpan EndTime { get; set; }

            [Required]
            [Column("status")]
            public string Status { get; set; } = "Pending";

            // Navigation Properties
            public virtual Users? User { get; set; }
            public virtual Facilities? Facility { get; set; }
        
    }
}
