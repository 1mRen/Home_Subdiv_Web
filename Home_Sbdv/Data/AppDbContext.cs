using Home_Sbdv.Entities;
using Microsoft.EntityFrameworkCore;

namespace Home_Sbdv.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Facilities> Facilities { get; set; }
        public DbSet<FacilityReservation> FacilityReservations { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<ContactDirectory> ContactDirectory { get; set; }
        public DbSet<VisitorPassRequest> VisitorPassRequests { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VisitorPassRequest>()
                .HasOne(v => v.RequestedBy)
                .WithMany()
                .HasForeignKey(v => v.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.SubmittedBy)
                .WithMany()
                .HasForeignKey(f => f.SubmittedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.AssignedTo)
                .WithMany()
                .HasForeignKey(f => f.AssignedToId)
                .IsRequired(false) // Since AssignedToId is nullable
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
