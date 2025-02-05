using Microsoft.EntityFrameworkCore;

namespace Home_Subdiv_Web.Controllers
{
    // Application Database Context class that inherits from DbContext
    public class AppDbContext : DbContext
    {
        // Constructor to initialize DbContext with options
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet representing the Users table in the database
        public DbSet<user> Users { get; set; }

        // Configuring the model during entity creation
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
