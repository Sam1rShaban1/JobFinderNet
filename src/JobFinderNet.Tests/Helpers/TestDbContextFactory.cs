using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Tests.Helpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"JobFinderTestDb_{Guid.NewGuid()}")
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static ApplicationUser CreateTestUser(string id, string email, string role)
    {
        return new ApplicationUser
        {
            Id = id,
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            CompanyName = role == "Employer" ? "Test Company" : null
        };
    }

    public static Job CreateTestJob(int id, string title, string employerId)
    {
        return new Job
        {
            Id = id,
            Title = title,
            Description = "Test description for the job position",
            CompanyName = "Test Corp",
            Location = "Remote",
            JobType = "Full-time",
            Salary = "$80,000/year",
            ExperienceRequired = "1-3 years",
            IsActive = true,
            PostedDate = DateTime.UtcNow,
            EmployerId = employerId,
            Employer = null!
        };
    }

    public static Application CreateTestApplication(int id, int jobId, string applicantId)
    {
        return new Application
        {
            Id = id,
            JobId = jobId,
            ApplicantId = applicantId,
            Status = ApplicationStatus.Pending,
            AppliedDate = DateTime.UtcNow,
            Job = null!,
            Applicant = null!
        };
    }
}
