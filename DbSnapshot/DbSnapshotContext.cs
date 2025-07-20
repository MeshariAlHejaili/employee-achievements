using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAchievementss.DbSnapshot;

public partial class DbSnapshotContext : DbContext
{
    public DbSnapshotContext()
    {
    }

    public DbSnapshotContext(DbContextOptions<DbSnapshotContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Achievement> Achievements { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<Manager> Managers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EmployeeAchievementsDB;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.HasIndex(e => e.OwnerId, "IX_Achievements_OwnerId");

            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Owner).WithMany(p => p.Achievements)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasIndex(e => e.AchievementId, "IX_Comments_AchievementId");

            entity.HasIndex(e => e.UserId, "IX_Comments_UserId");

            entity.Property(e => e.Content).HasMaxLength(1000);

            entity.HasOne(d => d.Achievement).WithMany(p => p.Comments).HasForeignKey(d => d.AchievementId);

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasIndex(e => e.AchievementId, "IX_Likes_AchievementId");

            entity.HasIndex(e => new { e.UserId, e.AchievementId }, "IX_Likes_UserId_AchievementId").IsUnique();

            entity.HasOne(d => d.Achievement).WithMany(p => p.Likes).HasForeignKey(d => d.AchievementId);

            entity.HasOne(d => d.User).WithMany(p => p.Likes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Manager>(entity =>
        {
            entity.HasIndex(e => e.DepartmentId, "IX_Managers_DepartmentId");

            entity.HasIndex(e => e.UserId, "IX_Managers_UserId").IsUnique();

            entity.HasOne(d => d.Department).WithMany(p => p.Managers)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithOne(p => p.Manager)
                .HasForeignKey<Manager>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.DepartmentId, "IX_Users_DepartmentId");

            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.HasIndex(e => e.ManagerId, "IX_Users_ManagerId");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.ProfilePicture).HasMaxLength(200);

            entity.HasOne(d => d.Department).WithMany(p => p.Users).HasForeignKey(d => d.DepartmentId);

            entity.HasOne(d => d.ManagerNavigation).WithMany(p => p.Users).HasForeignKey(d => d.ManagerId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
