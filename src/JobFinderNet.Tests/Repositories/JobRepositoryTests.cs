using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Infrastructure.Data;
using JobFinderNet.Infrastructure.Repositories;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Repositories;

public class JobRepositoryTests
{
    [Fact]
    public async Task CreateJobAsync_ShouldAddJobToDatabase()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new JobRepository(context);
        var employer = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.Add(employer);
        await context.SaveChangesAsync();

        var job = TestDbContextFactory.CreateTestJob(1, "Software Engineer", employer.Id);
        job.Employer = employer;

        await repo.CreateJobAsync(job);

        var savedJob = await context.Jobs.FirstOrDefaultAsync(j => j.Id == job.Id);
        Assert.NotNull(savedJob);
        Assert.Equal("Software Engineer", savedJob.Title);
        Assert.True(savedJob.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnJobWithIncludes()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new JobRepository(context);
        var employer = TestDbContextFactory.CreateTestUser("emp2", "emp2@test.com", "Employer");
        context.Users.Add(employer);
        await context.SaveChangesAsync();

        var job = TestDbContextFactory.CreateTestJob(2, "Backend Developer", employer.Id);
        job.Employer = employer;
        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        var result = await repo.GetByIdAsync(2);

        Assert.NotNull(result);
        Assert.Equal("Backend Developer", result.Title);
    }

    [Fact]
    public async Task SearchJobsAsync_ShouldReturnMatchingJobs()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new JobRepository(context);
        var employer = TestDbContextFactory.CreateTestUser("emp3", "emp3@test.com", "Employer");
        context.Users.Add(employer);
        await context.SaveChangesAsync();

        var jobs = new[]
        {
            CreateJob(3, "Software Engineer", "Tech Corp", employer.Id),
            CreateJob(4, "Data Analyst", "Data Corp", employer.Id),
            CreateJob(5, "Product Manager", "Tech Corp", employer.Id)
        };
        context.Jobs.AddRange(jobs);
        await context.SaveChangesAsync();

        var results = await repo.SearchJobsAsync("Engineer");

        Assert.Single(results);
        Assert.Contains(results, j => j.Title == "Software Engineer");
    }

    [Fact]
    public async Task DeleteJobAsync_ShouldRemoveJob()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new JobRepository(context);
        var employer = TestDbContextFactory.CreateTestUser("emp4", "emp4@test.com", "Employer");
        context.Users.Add(employer);
        var job = TestDbContextFactory.CreateTestJob(6, "To Delete", employer.Id);
        job.Employer = employer;
        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        await repo.DeleteJobAsync(6);

        Assert.Null(await context.Jobs.FindAsync(6));
    }

    private static Job CreateJob(int id, string title, string company, string empId) => new()
    {
        Id = id,
        Title = title,
        Description = $"Description for {title}",
        CompanyName = company,
        Location = "Remote",
        JobType = "Full-time",
        Salary = "$90,000/year",
        ExperienceRequired = "1-3 years",
        IsActive = true,
        PostedDate = DateTime.UtcNow,
        EmployerId = empId,
        Employer = null!
    };
}
