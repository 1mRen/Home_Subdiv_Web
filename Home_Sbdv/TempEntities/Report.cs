using System;
using System.Collections.Generic;

namespace Home_Sbdv.TempEntities;

public partial class Report
{
    public int ReportId { get; set; }

    public string ReportType { get; set; } = null!;

    public DateTime? GeneratedAt { get; set; }

    public string Data { get; set; } = null!;
}
