using JobPortalSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobPortalSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<JobSeeker> JobSeekers { get; set; }

        public DbSet<Job> Jobs { get; set; }

        public DbSet<Application> Applications { get; set; }

        public DbSet<EmployerProfile> EmployerProfiles { get; set; }

        // NEW
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Job>()
                .HasOne(j => j.Employer)
                .WithMany(e => e.Jobs)
                .HasForeignKey(j => j.EmployerId)
                .HasPrincipalKey(e => e.EmployerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Job>()
                .Property(j => j.Salary)
                .HasPrecision(18, 2);

            builder.Entity<Application>()
                .HasOne(a => a.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobId);

            builder.Entity<Application>()
                .HasOne(a => a.JobSeeker)
                .WithMany()
                .HasForeignKey(a => a.JobSeekerId);

            // ===========================
            // Notification Relationship
            // ===========================

            builder.Entity<Notification>()
                .HasOne(n => n.JobSeeker)
                .WithMany()
                .HasForeignKey(n => n.JobSeekerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}