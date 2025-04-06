using System;
using System.Collections.Generic;
using Home_Sbdv.TempEntities;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Home_Sbdv.TempData;

public partial class HomesubdivdbContext : DbContext
{
    public HomesubdivdbContext()
    {
    }

    public HomesubdivdbContext(DbContextOptions<HomesubdivdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Facility> Facilities { get; set; }

    public virtual DbSet<Facilityreservation> Facilityreservations { get; set; }

    public virtual DbSet<Forumpost> Forumposts { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Poll> Polls { get; set; }

    public virtual DbSet<Polloption> Polloptions { get; set; }

    public virtual DbSet<Pollvote> Pollvotes { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Servicerequest> Servicerequests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Visitorpass> Visitorpasses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql("name=Default", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.41-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasKey(e => e.AnnouncementId).HasName("PRIMARY");

            entity.ToTable("announcements");

            entity.HasIndex(e => e.PostedBy, "posted_by");

            entity.Property(e => e.AnnouncementId).HasColumnName("announcement_id");
            entity.Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.PostedBy).HasColumnName("posted_by");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.PostedByNavigation).WithMany(p => p.Announcements)
                .HasForeignKey(d => d.PostedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("announcements_ibfk_1");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PRIMARY");

            entity.ToTable("documents");

            entity.HasIndex(e => e.UploadedBy, "uploaded_by");

            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.FilePath)
                .HasColumnType("text")
                .HasColumnName("file_path");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("uploaded_at");
            entity.Property(e => e.UploadedBy).HasColumnName("uploaded_by");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.Documents)
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("documents_ibfk_1");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PRIMARY");

            entity.ToTable("events");

            entity.HasIndex(e => e.CreatedBy, "fk_events_users");

            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.EventDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("event_date");
            entity.Property(e => e.EventName)
                .HasMaxLength(255)
                .HasColumnName("event_name");
            entity.Property(e => e.LastUpdated)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("last_updated");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Events)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("events_ibfk_1");
        });

        modelBuilder.Entity<Facility>(entity =>
        {
            entity.HasKey(e => e.FacilityId).HasName("PRIMARY");

            entity.ToTable("facilities");

            entity.HasIndex(e => e.FacilityName, "facility_name").IsUnique();

            entity.Property(e => e.FacilityId).HasColumnName("facility_id");
            entity.Property(e => e.AvailabilityStatus)
                .HasDefaultValueSql("'Available'")
                .HasColumnType("enum('Available','Maintenance','Closed')")
                .HasColumnName("availability_status");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.FacilityName).HasColumnName("facility_name");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Facilityreservation>(entity =>
        {
            entity.HasKey(e => e.ReservationId).HasName("PRIMARY");

            entity.ToTable("facilityreservations");

            entity.HasIndex(e => e.FacilityId, "fk_facility");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.ReservationId).HasColumnName("reservation_id");
            entity.Property(e => e.EndTime)
                .HasColumnType("time")
                .HasColumnName("end_time");
            entity.Property(e => e.FacilityId).HasColumnName("facility_id");
            entity.Property(e => e.ReservationDate).HasColumnName("reservation_date");
            entity.Property(e => e.StartTime)
                .HasColumnType("time")
                .HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Pending'")
                .HasColumnType("enum('Pending','Approved','Rejected')")
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Facility).WithMany(p => p.Facilityreservations)
                .HasForeignKey(d => d.FacilityId)
                .HasConstraintName("fk_facility");

            entity.HasOne(d => d.User).WithMany(p => p.Facilityreservations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("facilityreservations_ibfk_1");
        });

        modelBuilder.Entity<Forumpost>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PRIMARY");

            entity.ToTable("forumposts");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Forumposts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("forumposts_ibfk_1");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PRIMARY");

            entity.ToTable("payments");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.PaidAt)
                .HasColumnType("timestamp")
                .HasColumnName("paid_at");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Pending'")
                .HasColumnType("enum('Pending','Paid','Overdue')")
                .HasColumnName("status");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(100)
                .HasColumnName("transaction_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("payments_ibfk_1");
        });

        modelBuilder.Entity<Poll>(entity =>
        {
            entity.HasKey(e => e.PollId).HasName("PRIMARY");

            entity.ToTable("polls");

            entity.HasIndex(e => e.CreatedBy, "created_by");

            entity.Property(e => e.PollId).HasColumnName("poll_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Question)
                .HasColumnType("text")
                .HasColumnName("question");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Polls)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("polls_ibfk_1");
        });

        modelBuilder.Entity<Polloption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PRIMARY");

            entity.ToTable("polloptions");

            entity.HasIndex(e => e.PollId, "poll_id");

            entity.Property(e => e.OptionId).HasColumnName("option_id");
            entity.Property(e => e.OptionText)
                .HasMaxLength(255)
                .HasColumnName("option_text");
            entity.Property(e => e.PollId).HasColumnName("poll_id");

            entity.HasOne(d => d.Poll).WithMany(p => p.Polloptions)
                .HasForeignKey(d => d.PollId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("polloptions_ibfk_1");
        });

        modelBuilder.Entity<Pollvote>(entity =>
        {
            entity.HasKey(e => e.VoteId).HasName("PRIMARY");

            entity.ToTable("pollvotes");

            entity.HasIndex(e => e.OptionId, "option_id");

            entity.HasIndex(e => e.PollId, "poll_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.VoteId).HasColumnName("vote_id");
            entity.Property(e => e.OptionId).HasColumnName("option_id");
            entity.Property(e => e.PollId).HasColumnName("poll_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Option).WithMany(p => p.Pollvotes)
                .HasForeignKey(d => d.OptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pollvotes_ibfk_2");

            entity.HasOne(d => d.Poll).WithMany(p => p.Pollvotes)
                .HasForeignKey(d => d.PollId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pollvotes_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.Pollvotes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pollvotes_ibfk_3");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PRIMARY");

            entity.ToTable("reports");

            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.Data)
                .HasColumnType("json")
                .HasColumnName("data");
            entity.Property(e => e.GeneratedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("generated_at");
            entity.Property(e => e.ReportType)
                .HasMaxLength(255)
                .HasColumnName("report_type");
        });

        modelBuilder.Entity<Servicerequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PRIMARY");

            entity.ToTable("servicerequests");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.RequestType)
                .HasMaxLength(255)
                .HasColumnName("request_type");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Open'")
                .HasColumnType("enum('Open','In Progress','Completed','Rejected')")
                .HasColumnName("status");
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("submitted_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Servicerequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("servicerequests_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.Username, "username").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(25)
                .HasColumnName("contact_number");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EmailVerificationExpiry)
                .HasColumnType("datetime")
                .HasColumnName("email_verification_expiry");
            entity.Property(e => e.EmailVerificationToken)
                .HasMaxLength(255)
                .HasColumnName("email_verification_token");
            entity.Property(e => e.EmailVerified).HasColumnName("email_verified");
            entity.Property(e => e.Firstname)
                .HasMaxLength(50)
                .HasColumnName("firstname");
            entity.Property(e => e.Gender)
                .HasColumnType("enum('male','female','others')")
                .HasColumnName("gender");
            entity.Property(e => e.Lastname)
                .HasMaxLength(50)
                .HasColumnName("lastname");
            entity.Property(e => e.LockoutEnd)
                .HasColumnType("datetime")
                .HasColumnName("lockout_end");
            entity.Property(e => e.LoginAttempts).HasColumnName("login_attempts");
            entity.Property(e => e.OwnershipStatus)
                .HasColumnType("enum('owned','rented')")
                .HasColumnName("ownership_status");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.PasswordResetExpiry)
                .HasColumnType("datetime")
                .HasColumnName("password_reset_expiry");
            entity.Property(e => e.PasswordResetToken)
                .HasMaxLength(255)
                .HasColumnName("password_reset_token");
            entity.Property(e => e.Role)
                .HasDefaultValueSql("'homeowner'")
                .HasColumnType("enum('admin','homeowner','staff')")
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Visitorpass>(entity =>
        {
            entity.HasKey(e => e.PassId).HasName("PRIMARY");

            entity.ToTable("visitorpasses");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.PassId).HasColumnName("pass_id");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Pending'")
                .HasColumnType("enum('Pending','Approved','Denied')")
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VehiclePlate)
                .HasMaxLength(50)
                .HasColumnName("vehicle_plate");
            entity.Property(e => e.VisitDate).HasColumnName("visit_date");
            entity.Property(e => e.VisitorName)
                .HasMaxLength(255)
                .HasColumnName("visitor_name");

            entity.HasOne(d => d.User).WithMany(p => p.Visitorpasses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("visitorpasses_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
