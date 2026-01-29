using Demo_web_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_web_MVC.Data.AppDatabase
{
    public class AppDatabase : DbContext
    {

        public AppDatabase(DbContextOptions<AppDatabase> options) : base(options)
        {
        }
        public DbSet<Demo_web_MVC.Models.User> Users { get; set; }
        public DbSet<Demo_web_MVC.Models.Contact> Contacts { get; set; }
        public DbSet<Demo_web_MVC.Models.UserToken> userTokens { get; set; }
        public DbSet<Demo_web_MVC.Models.Role> Roles { get; set; }
        public DbSet<Demo_web_MVC.Models.UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<UserToken>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tokens)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<UserToken>()
                .HasIndex(t => t.Token)
                .IsUnique();
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Code = "USER", Name = "Người dùng" },
                new Role { Id = 2, Code = "ADMIN", Name = "Quản trị" },
                new Role { Id = 3, Code = "STAFF", Name = "Nhân viên" }
            );
        }


    }
}
