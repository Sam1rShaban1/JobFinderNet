using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobFinderNet.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobApplication> Applications { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Remove duplicate ApplicationUser entity configuration
        builder.Entity<ApplicationUser>().ToTable("AspNetUsers");

        // Configure relationships
        builder.Entity<Job>()
            .HasOne(j => j.Employer)
            .WithMany()
            .HasForeignKey(j => j.EmployerId)
            .IsRequired();

        builder.Entity<JobApplication>()
            .HasOne(a => a.Applicant)
            .WithMany()
            .HasForeignKey(a => a.ApplicantId)
            .IsRequired();

        builder.Entity<JobApplication>()
            .HasOne(a => a.Job)
            .WithMany()
            .HasForeignKey(a => a.JobId)
            .IsRequired();
    }
}

public class ApplicationUser : IdentityUser
{
    // Add custom properties here
}
