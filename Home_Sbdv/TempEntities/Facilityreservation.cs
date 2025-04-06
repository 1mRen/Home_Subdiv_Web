using System;
using System.Collections.Generic;

namespace Home_Sbdv.TempEntities;

public partial class Facilityreservation
{
    public int ReservationId { get; set; }

    public int UserId { get; set; }

    public DateOnly ReservationDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Status { get; set; }

    public int FacilityId { get; set; }

    public virtual Facility Facility { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
