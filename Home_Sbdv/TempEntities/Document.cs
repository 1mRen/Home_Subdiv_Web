using System;
using System.Collections.Generic;

namespace Home_Sbdv.TempEntities;

public partial class Document
{
    public int DocumentId { get; set; }

    public string Title { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public int UploadedBy { get; set; }

    public DateTime? UploadedAt { get; set; }

    public virtual User UploadedByNavigation { get; set; } = null!;
}
