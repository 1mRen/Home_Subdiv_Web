using Home_Sbdv.Entities;
using Microsoft.EntityFrameworkCore;
using static Home_Sbdv.Entities.facilityReservation;

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
    }
}
