using System;
using System.Collections.Generic;

namespace Home_Sbdv.TempEntities;

public partial class Event
{
    public int EventId { get; set; }

    public string EventName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;
}
