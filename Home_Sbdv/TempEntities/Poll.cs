using System;
using System.Collections.Generic;

namespace Home_Sbdv.TempEntities;

public partial class Poll
{
    public int PollId { get; set; }

    public string Question { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Polloption> Polloptions { get; set; } = new List<Polloption>();

    public virtual ICollection<Pollvote> Pollvotes { get; set; } = new List<Pollvote>();
}
