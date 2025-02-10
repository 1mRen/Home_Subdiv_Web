using Home_Sbdv.Controllers;
using Microsoft.EntityFrameworkCore;

namespace Home_Sbdv.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
    }
}
