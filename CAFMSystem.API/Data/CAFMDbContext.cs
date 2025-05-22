using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CAFMSystem.API.Models;

namespace CAFMSystem.API.Data
{
    /// <summary>
    /// Database context for the CAFM system
    /// Extends IdentityDbContext to include ASP.NET Core Identity tables
    /// </summary>
    public class CAFMDbContext : IdentityDbContext<User>
    {
        public CAFMDbContext(DbContextOptions<CAFMDbContext> options) : base(options)
        {
        }

        // DbSets for our entities
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure User entity
            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Department).HasMaxLength(200);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                // Configure indexes for better performance
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Department);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure Ticket entity
            builder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(300);
                entity.Property(e => e.ImagePath).HasMaxLength(500);
                entity.Property(e => e.ExtractedKeywords).HasMaxLength(500);
                entity.Property(e => e.Priority).HasDefaultValue(2);
                entity.Property(e => e.Status).HasDefaultValue(TicketStatus.Open);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Configure relationships
                entity.HasOne(t => t.CreatedByUser)
                      .WithMany(u => u.CreatedTickets)
                      .HasForeignKey(t => t.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

                entity.HasOne(t => t.AssignedToUser)
                      .WithMany(u => u.AssignedTickets)
                      .HasForeignKey(t => t.AssignedToUserId)
                      .OnDelete(DeleteBehavior.SetNull); // Set to null if user is deleted

                // Configure indexes for better query performance
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.CreatedByUserId);
                entity.HasIndex(e => e.AssignedToUserId);
            });

            // Seed default roles
            SeedRoles(builder);
        }

        /// <summary>
        /// Seeds default roles for the CAFM system
        /// </summary>
        private void SeedRoles(ModelBuilder builder)
        {
            var roles = new[]
            {
                new { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new { Id = "2", Name = "AssetManager", NormalizedName = "ASSETMANAGER" },
                new { Id = "3", Name = "Plumber", NormalizedName = "PLUMBER" },
                new { Id = "4", Name = "Electrician", NormalizedName = "ELECTRICIAN" },
                new { Id = "5", Name = "Cleaner", NormalizedName = "CLEANER" },
                new { Id = "6", Name = "EndUser", NormalizedName = "ENDUSER" }
            };

            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().HasData(roles);
        }
    }
}
