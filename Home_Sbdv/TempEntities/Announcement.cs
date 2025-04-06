using System;
using System.Collections.Generic;

namespace Home_Sbdv.TempEntities;

public partial class Announcement
{
    public int AnnouncementId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int PostedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User PostedByNavigation { get; set; } = null!;
}
