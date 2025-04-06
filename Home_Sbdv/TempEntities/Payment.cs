using System;
using System.Collections.Generic;

namespace Home_Sbdv.TempEntities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public DateOnly DueDate { get; set; }

    public string? Status { get; set; }

    public string? PaymentMethod { get; set; }

    public string? TransactionId { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual User User { get; set; } = null!;
}
