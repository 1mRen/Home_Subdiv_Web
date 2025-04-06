using System;
using System.Collections.Generic;

namespace Home_Sbdv.TempEntities;

public partial class User
{
    public int UserId { get; set; }

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string ContactNumber { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string Role { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public string OwnershipStatus { get; set; } = null!;

    public int? LoginAttempts { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public bool EmailVerified { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetExpiry { get; set; }

    public string? EmailVerificationToken { get; set; }

    public DateTime? EmailVerificationExpiry { get; set; }

    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<Facilityreservation> Facilityreservations { get; set; } = new List<Facilityreservation>();

    public virtual ICollection<Forumpost> Forumposts { get; set; } = new List<Forumpost>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Poll> Polls { get; set; } = new List<Poll>();

    public virtual ICollection<Pollvote> Pollvotes { get; set; } = new List<Pollvote>();

    public virtual ICollection<Servicerequest> Servicerequests { get; set; } = new List<Servicerequest>();

    public virtual ICollection<Visitorpass> Visitorpasses { get; set; } = new List<Visitorpass>();
}
