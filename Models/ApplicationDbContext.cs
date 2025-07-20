using Microsoft.EntityFrameworkCore;

namespace EmployeeAchievementss.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Position).HasMaxLength(100);
                entity.Property(e => e.ProfilePicture).HasMaxLength(200);
                
                // Ensure email is unique
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Achievement entity
            modelBuilder.Entity<Achievement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
                
                // Configure relationship with User
                entity.HasOne(e => e.Owner)
                      .WithMany(u => u.Achievements)
                      .HasForeignKey(e => e.OwnerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Comment entity
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
                
                // Configure relationship with User
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Configure relationship with Achievement
                entity.HasOne(e => e.Achievement)
                      .WithMany(a => a.Comments)
                      .HasForeignKey(e => e.AchievementId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Like entity
            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Configure relationship with User
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Likes)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Configure relationship with Achievement
                entity.HasOne(e => e.Achievement)
                      .WithMany(a => a.Likes)
                      .HasForeignKey(e => e.AchievementId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Ensure a user can only like an achievement once
                entity.HasIndex(e => new { e.UserId, e.AchievementId }).IsUnique();
            });

            // Configure Manager entity
            modelBuilder.Entity<Manager>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithOne()
                      .HasForeignKey<Manager>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Department)
                      .WithMany()
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Department entity
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // Seed Departments
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "تطوير البرمجيات" },
                new Department { Id = 2, Name = "إدارة المنتج" }
            );

            // Seed Managers as users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 10,
                    Name = "مدير البرمجيات",
                    Email = "manager1@amana.com",
                    Password = "test123",
                    DepartmentId = 1,
                    Position = "مدير قسم البرمجيات",
                    CreatedAt = new DateTime(2024, 7, 17, 8, 0, 0),
                    ManagerId = null
                },
                new User
                {
                    Id = 11,
                    Name = "مدير المنتج",
                    Email = "manager2@amana.com",
                    Password = "test123",
                    DepartmentId = 2,
                    Position = "مدير قسم المنتج",
                    CreatedAt = new DateTime(2024, 7, 17, 8, 0, 0),
                    ManagerId = null
                },
                // Employees (ManagerId will be set via SQL after migration)
                new User
                {
                    Id = 1,
                    Name = "مشاري الحربي",
                    Email = "mashari@amana.com",
                    Password = "123456",
                    DepartmentId = 1,
                    Position = "مطور برمجيات",
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0),
                    ManagerId = null
                },
                new User
                {
                    Id = 2,
                    Name = "سارة أحمد",
                    Email = "sara@amana.com",
                    Password = "123456",
                    DepartmentId = 2,
                    Position = "مدير منتج",
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0),
                    ManagerId = null
                },
                new User
                {
                    Id = 3,
                    Name = "أحمد محمد",
                    Email = "ahmed@amana.com",
                    Password = "123456",
                    DepartmentId = 1,
                    Position = "مطور خلفي",
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0),
                    ManagerId = null
                }
            );

            // Seed Managers
            modelBuilder.Entity<Manager>().HasData(
                new Manager { Id = 1, UserId = 10, DepartmentId = 1 },
                new Manager { Id = 2, UserId = 11, DepartmentId = 2 }
            );

            // Seed Achievements
            modelBuilder.Entity<Achievement>().HasData(
                new Achievement
                {
                    Id = 1,
                    Title = "مشروع نيوسواك",
                    Description = "إعادة تصميم كاملة لتدفق تأهيل المستخدمين لمشروع نيوسواك، مما أدى إلى تحسين تجربة المستخدم بنسبة 40% وتقليل وقت التدريب إلى النصف.",
                    Date = new DateTime(2024, 6, 25),
                    OwnerId = 1,
                    Status = "Pending",
                    CreatedAt = new DateTime(2024, 6, 2, 9, 0, 0)
                },
                new Achievement
                {
                    Id = 2,
                    Title = "تطوير نظام التقارير",
                    Description = "تطوير نظام تقارير جديد للإدارة يوفر رؤى شاملة عن أداء الفريق ومؤشرات الأداء الرئيسية.",
                    Date = new DateTime(2024, 6, 20),
                    OwnerId = 2,
                    Status = "Pending",
                    CreatedAt = new DateTime(2024, 6, 2, 9, 0, 0)
                },
                new Achievement
                {
                    Id = 3,
                    Title = "تحسين أداء النظام",
                    Description = "تحسين أداء النظام الأساسي بنسبة 60% من خلال تحسين قاعدة البيانات وتحسين الخوارزميات.",
                    Date = new DateTime(2024, 6, 18),
                    OwnerId = 3,
                    Status = "Pending",
                    CreatedAt = new DateTime(2024, 6, 2, 9, 0, 0)
                }
            );

            // Seed Comments
            modelBuilder.Entity<Comment>().HasData(
                new Comment
                {
                    Id = 1,
                    Content = "عمل رائع! هذا سيساعد كثيراً في تحسين تجربة المستخدمين.",
                    Date = new DateTime(2024, 6, 25, 10, 30, 0),
                    UserId = 2,
                    AchievementId = 1,
                    CreatedAt = new DateTime(2024, 6, 3, 10, 0, 0)
                },
                new Comment
                {
                    Id = 2,
                    Content = "أحسنت! هذا التطوير سيحدث فرقاً كبيراً.",
                    Date = new DateTime(2024, 6, 25, 11, 15, 0),
                    UserId = 3,
                    AchievementId = 1,
                    CreatedAt = new DateTime(2024, 6, 3, 10, 0, 0)
                },
                new Comment
                {
                    Id = 3,
                    Content = "ممتاز! هذا النظام سيوفر لنا رؤية واضحة عن الأداء.",
                    Date = new DateTime(2024, 6, 20, 14, 20, 0),
                    UserId = 1,
                    AchievementId = 2,
                    CreatedAt = new DateTime(2024, 6, 3, 10, 0, 0)
                }
            );

            // Seed Likes
            modelBuilder.Entity<Like>().HasData(
                new Like
                {
                    Id = 1,
                    Date = new DateTime(2024, 6, 25, 9, 0, 0),
                    UserId = 2,
                    AchievementId = 1,
                    CreatedAt = new DateTime(2024, 6, 4, 11, 0, 0)
                },
                new Like
                {
                    Id = 2,
                    Date = new DateTime(2024, 6, 25, 9, 30, 0),
                    UserId = 3,
                    AchievementId = 1,
                    CreatedAt = new DateTime(2024, 6, 4, 11, 0, 0)
                },
                new Like
                {
                    Id = 3,
                    Date = new DateTime(2024, 6, 20, 10, 0, 0),
                    UserId = 1,
                    AchievementId = 2,
                    CreatedAt = new DateTime(2024, 6, 4, 11, 0, 0)
                }
            );
        }
    }
} 