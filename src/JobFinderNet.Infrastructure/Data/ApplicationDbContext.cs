using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;

namespace JobFinderNet.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Job> Jobs { get; set; } = null!;
    public DbSet<Application> Applications { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<PendingDigest> PendingDigests { get; set; } = null!;
    public DbSet<SavedJob> SavedJobs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("users");
        builder.Entity<Job>().ToTable("jobs");
        builder.Entity<Application>().ToTable("applications");
        builder.Entity<UserProfile>().ToTable("user_profiles");
        builder.Entity<PendingDigest>().ToTable("pending_digests");

        builder.Entity<Job>()
            .Property(j => j.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Entity<Job>()
            .Property(j => j.Description)
            .HasColumnType("text")
            .IsRequired();

        builder.Entity<Job>()
            .Property(j => j.CompanyName)
            .HasMaxLength(200);

        builder.Entity<Job>()
            .Property(j => j.Location)
            .HasMaxLength(500);

        builder.Entity<Job>()
            .HasIndex(j => j.ExternalJobId)
            .IsUnique()
            .HasFilter("\"ExternalJobId\" IS NOT NULL");

        builder.Entity<Job>()
            .HasOne(j => j.Employer)
            .WithMany()
            .HasForeignKey(j => j.EmployerId)
            .IsRequired();

        builder.Entity<Application>()
            .HasOne(a => a.Job)
            .WithMany(j => j.Applications)
            .HasForeignKey(a => a.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Application>()
            .HasOne(a => a.Applicant)
            .WithMany()
            .HasForeignKey(a => a.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserProfile>()
            .HasIndex(u => u.UserId)
            .IsUnique();

        builder.Entity<UserProfile>()
            .Property(u => u.Skills)
            .HasColumnType("jsonb");

        builder.Entity<PendingDigest>()
            .HasOne(d => d.Job)
            .WithMany()
            .HasForeignKey(d => d.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PendingDigest>()
            .HasIndex(d => new { d.UserId, d.EmailFrequency });

        builder.Entity<SavedJob>().ToTable("saved_jobs");

        builder.Entity<SavedJob>()
            .HasOne(s => s.Job)
            .WithMany()
            .HasForeignKey(s => s.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SavedJob>()
            .HasIndex(s => new { s.UserId, s.JobId })
            .IsUnique();
    }
}
