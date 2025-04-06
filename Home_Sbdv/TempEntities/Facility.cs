using System;
using System.Collections.Generic;

namespace Home_Sbdv.TempEntities;

public partial class Facility
{
    public int FacilityId { get; set; }

    public string FacilityName { get; set; } = null!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public int? Capacity { get; set; }

    public string? AvailabilityStatus { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Facilityreservation> Facilityreservations { get; set; } = new List<Facilityreservation>();
}
