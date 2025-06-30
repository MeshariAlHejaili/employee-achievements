using Microsoft.EntityFrameworkCore;

namespace EmployeeAchievementss.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
    }
} 