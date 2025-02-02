using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JobFinderNet.Models;

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

        // Set table names with correct casing for PostgreSQL
        builder.Entity<ApplicationUser>().ToTable("users");
        builder.Entity<Job>().ToTable("jobs");
        builder.Entity<JobApplication>().ToTable("applications");

        // Configure relationships
        builder.Entity<Job>()
            .Property(j => j.Title)
            .HasMaxLength(100)
            .IsRequired();

        builder.Entity<Job>()
            .Property(j => j.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Entity<Job>()
            .HasOne(j => j.Employer)
            .WithMany()
            .HasForeignKey(j => j.EmployerId)
            .IsRequired();

        builder.Entity<JobApplication>()
            .HasOne(a => a.Job)
            .WithMany(j => j.Applications)
            .HasForeignKey(a => a.JobId)
            .IsRequired();

        builder.Entity<JobApplication>()
            .HasOne(a => a.Applicant)
            .WithMany()
            .HasForeignKey(a => a.ApplicantId)
            .IsRequired();
    }
}

public class ApplicationUser : IdentityUser
{
    // Add custom properties here
}
