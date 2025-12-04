using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using COMP2138_ICE.Models;
using Microsoft.AspNetCore.Identity;

namespace COMP2138_ICE.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings => 
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        // DbSets for your models
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Roles
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "2", Name = "Organizer", NormalizedName = "ORGANIZER" },
                new IdentityRole { Id = "3", Name = "Attendee", NormalizedName = "ATTENDEE" }
            );

            // Seed Admin User
            var adminId = "a18be9c0-aa65-4af8-bd17-00bd9344e575";
            var hasher = new PasswordHasher<ApplicationUser>();
            modelBuilder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = adminId,
                    UserName = "admin@event.com",
                    NormalizedUserName = "ADMIN@EVENT.COM",
                    Email = "admin@event.com",
                    NormalizedEmail = "ADMIN@EVENT.COM",
                    EmailConfirmed = true,
                    PasswordHash = hasher.HashPassword(null, "Admin@123"),
                    SecurityStamp = string.Empty
                }
            );

            // Assign Admin Role to Admin User
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = "1", // Admin Role Id
                    UserId = adminId
                }
            );

            // Seed categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Webinar" },
                new Category { Id = 2, Name = "Concert" },
                new Category { Id = 3, Name = "Workshop" },
                new Category { Id = 4, Name = "Conference" }
            );

            // Seed events with UTC dates
            // Note: We need to assign an OrganizerId to these events now. 
            // For simplicity, we'll assign them to the seeded Admin user.
            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    Id = 1,
                    Title = "ASP.NET Core Workshop",
                    CategoryId = 3,
                    DateTime = new DateTime(2025, 12, 4, 22, 48, 42, 113, DateTimeKind.Utc).AddTicks(1733),
                    Price = 50.0m,
                    TicketsAvailable = 20,
                    OrganizerId = adminId // Assigned to Admin
                },
                new Event
                {
                    Id = 2,
                    Title = "Virtual Concert",
                    CategoryId = 2,
                    DateTime = new DateTime(2025, 12, 12, 22, 48, 42, 124, DateTimeKind.Utc).AddTicks(9207),
                    Price = 100.0m,
                    TicketsAvailable = 10,
                    OrganizerId = adminId // Assigned to Admin
                }
            );
        }
    }
}