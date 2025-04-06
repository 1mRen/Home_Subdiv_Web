using System;
using System.Collections.Generic;

namespace Home_Sbdv.TempEntities;

public partial class Forumpost
{
    public int PostId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
