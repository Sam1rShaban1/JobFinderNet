using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Infrastructure.Repositories;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Repositories;

public class SavedJobRepositoryTests
{
    [Fact]
    public async Task GetUserSavedJobsAsync_ReturnsSavedJobs()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedJobRepository(context);

        var emp = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.Add(emp);
        var job = TestDbContextFactory.CreateTestJob(1, "Job1", emp.Id);
        job.Employer = emp;
        context.Jobs.Add(job);

        context.SavedJobs.AddRange(
            new SavedJob { UserId = "u1", JobId = 1, Job = job },
            new SavedJob { UserId = "u1", JobId = 1, Job = job }
        );
        await context.SaveChangesAsync();

        var results = await repo.GetUserSavedJobsAsync("u1");

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetUserSavedJobsAsync_Empty_ReturnsEmpty()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedJobRepository(context);

        var results = await repo.GetUserSavedJobsAsync("u1");

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetUserSavedJobIdsAsync_ReturnsIds()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedJobRepository(context);

        var emp = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.Add(emp);
        var job1 = TestDbContextFactory.CreateTestJob(1, "Job1", emp.Id);
        var job2 = TestDbContextFactory.CreateTestJob(2, "Job2", emp.Id);
        context.Jobs.AddRange(job1, job2);

        context.SavedJobs.AddRange(
            new SavedJob { UserId = "u1", JobId = 1, Job = job1 },
            new SavedJob { UserId = "u1", JobId = 2, Job = job2 }
        );
        await context.SaveChangesAsync();

        var ids = await repo.GetUserSavedJobIdsAsync("u1");

        Assert.Equal(2, ids.Count);
        Assert.Contains(1, ids);
    }

    [Fact]
    public async Task GetAsync_ReturnsSavedJob()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedJobRepository(context);

        var emp = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.Add(emp);
        var job = TestDbContextFactory.CreateTestJob(1, "Job1", emp.Id);
        context.Jobs.Add(job);

        var saved = new SavedJob { UserId = "u1", JobId = 1, Job = job };
        context.SavedJobs.Add(saved);
        await context.SaveChangesAsync();

        var result = await repo.GetAsync("u1", 1);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAsync_NotFound_ReturnsNull()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedJobRepository(context);

        var result = await repo.GetAsync("u1", 999);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedJobRepository(context);

        var emp = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.Add(emp);
        var job = TestDbContextFactory.CreateTestJob(1, "Job1", emp.Id);
        context.Jobs.Add(job);

        context.SavedJobs.Add(new SavedJob { UserId = "u1", JobId = 1, Job = job });
        await context.SaveChangesAsync();

        var exists = await repo.ExistsAsync("u1", 1);

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedJobRepository(context);

        var exists = await repo.ExistsAsync("u1", 999);

        Assert.False(exists);
    }

    [Fact]
    public async Task AddAsync_AddsAndSaves()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedJobRepository(context);

        var emp = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.Add(emp);
        var job = TestDbContextFactory.CreateTestJob(1, "Job1", emp.Id);
        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        await repo.AddAsync(new SavedJob { UserId = "u1", JobId = 1, Job = job });
        await repo.SaveChangesAsync();

        Assert.Equal(1, await context.SavedJobs.CountAsync());
    }

    [Fact]
    public async Task Remove_RemovesSavedJob()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedJobRepository(context);

        var emp = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.Add(emp);
        var job = TestDbContextFactory.CreateTestJob(1, "Job1", emp.Id);
        context.Jobs.Add(job);

        var saved = new SavedJob { UserId = "u1", JobId = 1, Job = job };
        context.SavedJobs.Add(saved);
        await context.SaveChangesAsync();

        repo.Remove(saved);
        await repo.SaveChangesAsync();

        Assert.Equal(0, await context.SavedJobs.CountAsync());
    }
}
