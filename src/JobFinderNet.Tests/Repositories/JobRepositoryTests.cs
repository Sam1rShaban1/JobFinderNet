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

    [Fact]
    public async Task GetEmployerJobsAsync_ReturnsEmployerJobs()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new JobRepository(context);
        var emp = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.Add(emp);
        context.Jobs.AddRange(
            CreateJob(10, "Job A", "C1", emp.Id),
            CreateJob(11, "Job B", "C1", emp.Id)
        );
        await context.SaveChangesAsync();

        var jobs = await repo.GetEmployerJobsAsync("emp1");

        Assert.Equal(2, jobs.Count);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsTotalCount()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new JobRepository(context);
        var emp = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.Add(emp);
        context.Jobs.AddRange(
            CreateJob(12, "Job A", "C1", emp.Id),
            CreateJob(13, "Job B", "C1", emp.Id)
        );
        await context.SaveChangesAsync();

        var count = await repo.GetCountAsync();

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetAllActiveJobsAsync_ReturnsOnlyActive()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new JobRepository(context);
        var emp = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.Add(emp);
        var active = CreateJob(14, "Active", "C1", emp.Id);
        var inactive = CreateJob(15, "Inactive", "C1", emp.Id);
        inactive.IsActive = false;
        context.Jobs.AddRange(active, inactive);
        await context.SaveChangesAsync();

        var jobs = await repo.GetAllActiveJobsAsync();

        Assert.Single(jobs);
        Assert.Equal("Active", jobs[0].Title);
    }

    [Fact]
    public async Task GetJobsByTypeAsync_ReturnsCounts()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new JobRepository(context);
        var emp = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.Add(emp);
        var ft = CreateJob(16, "FT Job", "C1", emp.Id);
        ft.JobType = "Full-time";
        var ct = CreateJob(17, "CT Job", "C1", emp.Id);
        ct.JobType = "Contract";
        context.Jobs.AddRange(ft, ct);
        await context.SaveChangesAsync();

        var byType = await repo.GetJobsByTypeAsync();

        Assert.Equal(2, byType.Count);
        Assert.Equal(1, byType["Full-time"]);
    }

    [Fact]
    public async Task GetPaginatedJobsAsync_ReturnsPage()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new JobRepository(context);
        var emp = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.Add(emp);
        for (int i = 18; i < 24; i++)
            context.Jobs.Add(CreateJob(i, $"Job {i}", "C1", emp.Id));
        await context.SaveChangesAsync();

        var page = await repo.GetPaginatedJobsAsync(1, 3);

        Assert.Equal(3, page.Items.Count);
        Assert.Equal(6, page.TotalCount);
        Assert.True(page.HasNextPage);
    }

    [Fact]
    public async Task UpdateJobAsync_UpdatesJob()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new JobRepository(context);
        var emp = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.Add(emp);
        var job = CreateJob(24, "Original", "C1", emp.Id);
        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        job.Title = "Updated";
        await repo.UpdateJobAsync(job);

        var saved = await context.Jobs.FindAsync(24);
        Assert.NotNull(saved);
        Assert.Equal("Updated", saved.Title);
    }

    [Fact]
    public async Task ToggleJobStatusAsync_TogglesActive()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new JobRepository(context);
        var emp = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.Add(emp);
        var job = CreateJob(25, "Toggle", "C1", emp.Id);
        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        await repo.ToggleJobStatusAsync(25);

        var saved = await context.Jobs.FindAsync(25);
        Assert.False(saved!.IsActive);

        await repo.ToggleJobStatusAsync(25);
        Assert.True((await context.Jobs.FindAsync(25))!.IsActive);
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
