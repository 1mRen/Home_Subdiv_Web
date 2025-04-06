using System;
using System.Collections.Generic;

namespace Home_Sbdv.TempEntities;

public partial class Servicerequest
{
    public int RequestId { get; set; }

    public int UserId { get; set; }

    public string RequestType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
