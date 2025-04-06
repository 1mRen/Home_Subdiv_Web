using System;
using System.Collections.Generic;

namespace Home_Sbdv.TempEntities;

public partial class Polloption
{
    public int OptionId { get; set; }

    public int PollId { get; set; }

    public string OptionText { get; set; } = null!;

    public virtual Poll Poll { get; set; } = null!;

    public virtual ICollection<Pollvote> Pollvotes { get; set; } = new List<Pollvote>();
}
