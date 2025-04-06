using System;
using System.Collections.Generic;

namespace Home_Sbdv.TempEntities;

public partial class Visitorpass
{
    public int PassId { get; set; }

    public int UserId { get; set; }

    public string VisitorName { get; set; } = null!;

    public string? VehiclePlate { get; set; }

    public DateOnly VisitDate { get; set; }

    public string? Status { get; set; }

    public virtual User User { get; set; } = null!;
}
