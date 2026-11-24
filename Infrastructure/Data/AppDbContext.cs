using Domain.Entities;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    /// <summary>
    /// Database context for activities, registrations, users and roles.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<Registration> Registrations => Set<Registration>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Activities and registrations
            modelBuilder.Entity<Activity>(entity =>
            {
                entity.ToTable("activities");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).IsRequired();
                entity.Property(a => a.Location).IsRequired();
                entity.Property(a => a.Description).IsRequired();
                entity.Property(a => a.StartTime).IsRequired();
                entity.Property(a => a.EndTime).IsRequired();
                entity.Property(a => a.IncludesFood).HasDefaultValue(false);
                entity.Property(a => a.Cost).HasColumnType("decimal(10,2)").HasDefaultValue(0m);
                entity.Ignore(a => a.IsDeleted);

                // Deleted activities stay in the database, but every query leaves them out,
                // so nobody sees them anymore, administrators included.
                entity.HasQueryFilter(a => a.DeletedAt == null);
            });

            modelBuilder.Entity<Registration>(entity =>
            {
                // A registration belongs either to an employee (UserId) or to a visitor without an account (the Visitor columns).
                entity.ToTable("registrations", table => table.HasCheckConstraint(
                    "CK_registrations_participant",
                    "\"UserId\" IS NOT NULL OR (\"VisitorName\" IS NOT NULL AND \"VisitorEmail\" IS NOT NULL AND \"VisitorPhone\" IS NOT NULL AND \"CancellationCodeHash\" IS NOT NULL)"));
                entity.HasKey(r => r.Id);
                entity.HasOne(r => r.Activity).WithMany(a => a.Registrations).HasForeignKey(r => r.ActivityId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<User>().WithMany(user => user.Registrations).HasForeignKey(r => r.UserId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
                entity.Property(r => r.VisitorName).HasMaxLength(200);
                entity.Property(r => r.VisitorEmail).HasMaxLength(320);
                entity.Property(r => r.VisitorPhone).HasMaxLength(30);
                entity.Ignore(r => r.IsVisitor);

                // A visitor can register only once per activity with the same e-mail address.
                entity.HasIndex(r => new { r.ActivityId, r.VisitorEmail }).IsUnique().HasFilter("\"VisitorEmail\" IS NOT NULL");

                // Registrations of a deleted activity disappear together with the activity.
                entity.HasQueryFilter(r => r.Activity!.DeletedAt == null);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(user => user.Id);
                entity.Property(user => user.Name).HasMaxLength(200).IsRequired();
                entity.Property(user => user.JobTitle).HasMaxLength(100).IsRequired();
                entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
                entity.Property(user => user.PasswordHash).IsRequired();
                // Only active users need a unique email address, so the address of a deleted user can be used again.
                entity.HasIndex(user => user.Email).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
                entity.HasOne(user => user.Role)
                    .WithMany(role => role.Users)
                    .HasForeignKey(user => user.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Deleted users stay in the database, but every query leaves them out: they cannot log in,
                // their tokens are rejected and they no longer appear in any list.
                entity.HasQueryFilter(user => user.DeletedAt == null);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(role => role.Id);
                entity.Property(role => role.Name).HasMaxLength(50).IsRequired();
                entity.Property(role => role.Description).HasMaxLength(250).IsRequired();
                entity.HasIndex(role => role.Name).IsUnique();
            });

            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // --- Activities ---
            modelBuilder.Entity<Activity>().HasData(
                new Activity { Id = new Guid("10000000-0000-4000-8000-000000000001"), Name = "LAN Party", Location = "Headquarters - Room A", Description = "Overnight LAN party met snacks en toernooien.", StartTime = baseDate.AddDays(30), EndTime = baseDate.AddDays(31), IncludesFood = true, Cost = 5.00m, MaxParticipants = 50, MinParticipants = 5, ImageUrl = null, Requirements = "Bring your own PC and ethernet cable" },
                new Activity { Id = new Guid("10000000-0000-4000-8000-000000000002"), Name = "Pizza Avond", Location = "Community Kitchen", Description = "Gezellige pizza avond met bordspellen.", StartTime = baseDate.AddDays(10), EndTime = baseDate.AddDays(10).AddHours(3), IncludesFood = true, Cost = 3.50m, MaxParticipants = 20, MinParticipants = 4, ImageUrl = null, Requirements = "Let us know dietary restrictions" },
                new Activity { Id = new Guid("10000000-0000-4000-8000-000000000003"), Name = "Hardloop Sessie", Location = "Stadspark", Description = "Vrijwillige hardloop training voor alle niveaus.", StartTime = baseDate.AddDays(5).AddHours(18), EndTime = baseDate.AddDays(5).AddHours(19), IncludesFood = false, Cost = 0.00m, MaxParticipants = 30, MinParticipants = 2, ImageUrl = null, Requirements = "Sportschoenen aanbevolen" }
            );

            // Fixed hashes keep seed metadata deterministic; never hash passwords while building the model.
            // --- Roles and users ---
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = RoleIds.Administrator, Name = RoleNames.Administrator, Description = "Beheerder met volledige toegang." },
                new Role { Id = RoleIds.User, Name = RoleNames.User, Description = "Gebruiker met standaardtoegang." });

            modelBuilder.Entity<User>().HasData(
                new User { Id = new Guid("20000000-0000-4000-8000-000000000001"), Name = "Test Beheerder", JobTitle = JobTitles.Facilitair, Email = "test@actievadis.nl", PasswordHash = "AQAAAAIAAzRQAAAAEAzwOW9EbuKg9OnBsmsNm476hyrb9xt5nTbSE8qchnaHeB9G1uz0KAeaFLQOdlXsfQ==", RoleId = RoleIds.Administrator },
                new User { Id = new Guid("20000000-0000-4000-8000-000000000002"), Name = "Sanne Jansen", JobTitle = JobTitles.Ontwikkelaar1, Email = "sanne.jansen@actievadis.nl", PasswordHash = "AQAAAAIAAzRQAAAAEAzwOW9EbuKg9OnBsmsNm476hyrb9xt5nTbSE8qchnaHeB9G1uz0KAeaFLQOdlXsfQ==", RoleId = RoleIds.User },
                new User { Id = new Guid("20000000-0000-4000-8000-000000000003"), Name = "Daan de Vries", JobTitle = JobTitles.Sales, Email = "daan.devries@actievadis.nl", PasswordHash = "AQAAAAIAAzRQAAAAEAzwOW9EbuKg9OnBsmsNm476hyrb9xt5nTbSE8qchnaHeB9G1uz0KAeaFLQOdlXsfQ==", RoleId = RoleIds.User },
                new User { Id = new Guid("20000000-0000-4000-8000-000000000005"), Name = "Mila Bakker", JobTitle = JobTitles.HR, Email = "mila.bakker@actievadis.nl", PasswordHash = "AQAAAAIAAzRQAAAAEAzwOW9EbuKg9OnBsmsNm476hyrb9xt5nTbSE8qchnaHeB9G1uz0KAeaFLQOdlXsfQ==", RoleId = RoleIds.User });

            modelBuilder.Entity<Registration>().HasData(
                new Registration { Id = new Guid("30000000-0000-4000-8000-000000000001"), ActivityId = new Guid("10000000-0000-4000-8000-000000000001"), UserId = new Guid("20000000-0000-4000-8000-000000000002"), Comment = "Breng m'n headset mee", RegisteredAt = baseDate.AddDays(15) },
                new Registration { Id = new Guid("30000000-0000-4000-8000-000000000002"), ActivityId = new Guid("10000000-0000-4000-8000-000000000001"), UserId = new Guid("20000000-0000-4000-8000-000000000003"), Comment = "Doe mee aan FPS toernooi", RegisteredAt = baseDate.AddDays(16) },
                new Registration { Id = new Guid("30000000-0000-4000-8000-000000000003"), ActivityId = new Guid("10000000-0000-4000-8000-000000000002"), UserId = new Guid("20000000-0000-4000-8000-000000000005"), Comment = "Vegetarisch graag", RegisteredAt = baseDate.AddDays(7) }
            );
        }
    }
}
