using HallDiningTokenSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HallDiningTokenSystem.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure ApplicationUser properties
            builder.Entity<ApplicationUser>()
                .Property(u => u.FullName)
                .HasMaxLength(100);

            builder.Entity<ApplicationUser>()
                .Property(u => u.StudentId)
                .HasMaxLength(50);

            builder.Entity<ApplicationUser>()
                .Property(u => u.Department)
                .HasMaxLength(100);

            builder.Entity<ApplicationUser>()
                .Property(u => u.Batch)
                .HasMaxLength(50);

            // Add unique filtered index for StudentId (only where not null)
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.StudentId)
                .IsUnique()
                .HasFilter("[StudentId] IS NOT NULL");
        }
    }
}
