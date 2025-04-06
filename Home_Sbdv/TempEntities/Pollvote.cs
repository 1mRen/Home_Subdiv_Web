using System;
using System.Collections.Generic;

namespace Home_Sbdv.TempEntities;

public partial class Pollvote
{
    public int VoteId { get; set; }

    public int PollId { get; set; }

    public int OptionId { get; set; }

    public int UserId { get; set; }

    public virtual Polloption Option { get; set; } = null!;

    public virtual Poll Poll { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
