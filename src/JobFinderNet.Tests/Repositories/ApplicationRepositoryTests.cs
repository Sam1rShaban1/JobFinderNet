using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Infrastructure.Repositories;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Repositories;

public class ApplicationRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddApplication()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new ApplicationRepository(context);
        var applicant = TestDbContextFactory.CreateTestUser("app1", "app@test.com", "Applicant");
        var employer = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.AddRange(applicant, employer);

        var job = TestDbContextFactory.CreateTestJob(1, "Test Job", employer.Id);
        job.Employer = employer;
        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        var application = new Application
        {
            JobId = 1,
            Job = job,
            ApplicantId = applicant.Id,
            Applicant = applicant,
            Status = ApplicationStatus.Pending,
            AppliedDate = DateTime.UtcNow
        };

        var result = await repo.AddAsync(application);

        Assert.True(result);
        Assert.Equal(1, await context.Applications.CountAsync());
    }

    [Fact]
    public async Task HasUserAppliedToJob_ShouldReturnTrueIfApplied()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new ApplicationRepository(context);
        var applicant = TestDbContextFactory.CreateTestUser("app2", "app2@test.com", "Applicant");
        var employer = TestDbContextFactory.CreateTestUser("emp2", "emp2@test.com", "Employer");
        context.Users.AddRange(applicant, employer);

        var job = TestDbContextFactory.CreateTestJob(2, "Job 2", employer.Id);
        job.Employer = employer;
        context.Jobs.Add(job);

        var application = new Application
        {
            JobId = 2,
            Job = job,
            ApplicantId = applicant.Id,
            Applicant = applicant,
            Status = ApplicationStatus.Pending,
            AppliedDate = DateTime.UtcNow
        };
        context.Applications.Add(application);
        await context.SaveChangesAsync();

        var hasApplied = await repo.HasUserAppliedToJob(applicant.Id, 2);

        Assert.True(hasApplied);
    }

    [Fact]
    public async Task GetUserApplicationsAsync_ShouldReturnUserApplications()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new ApplicationRepository(context);
        var applicant = TestDbContextFactory.CreateTestUser("app3", "app3@test.com", "Applicant");
        var employer = TestDbContextFactory.CreateTestUser("emp3", "emp3@test.com", "Employer");
        context.Users.AddRange(applicant, employer);

        var job1 = TestDbContextFactory.CreateTestJob(3, "Job 3", employer.Id);
        job1.Employer = employer;
        var job2 = TestDbContextFactory.CreateTestJob(4, "Job 4", employer.Id);
        job2.Employer = employer;
        context.Jobs.AddRange(job1, job2);

        context.Applications.AddRange(
            new Application { JobId = 3, Job = job1, ApplicantId = applicant.Id, Applicant = applicant, Status = ApplicationStatus.Pending, AppliedDate = DateTime.UtcNow },
            new Application { JobId = 4, Job = job2, ApplicantId = applicant.Id, Applicant = applicant, Status = ApplicationStatus.Pending, AppliedDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var apps = await repo.GetUserApplicationsAsync(applicant.Id);

        Assert.Equal(2, apps.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsApplication()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new ApplicationRepository(context);
        var applicant = TestDbContextFactory.CreateTestUser("app4", "app4@test.com", "Applicant");
        var employer = TestDbContextFactory.CreateTestUser("emp4", "emp4@test.com", "Employer");
        context.Users.AddRange(applicant, employer);

        var job = TestDbContextFactory.CreateTestJob(5, "Job 5", employer.Id);
        job.Employer = employer;
        context.Jobs.Add(job);

        var app = new Application
        {
            Id = 1, JobId = 5, Job = job, ApplicantId = applicant.Id, Applicant = applicant,
            Status = ApplicationStatus.Pending, AppliedDate = DateTime.UtcNow
        };
        context.Applications.Add(app);
        await context.SaveChangesAsync();

        var result = await repo.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(5, result.JobId);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new ApplicationRepository(context);

        var result = await repo.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new ApplicationRepository(context);
        var applicant = TestDbContextFactory.CreateTestUser("app5", "app5@test.com", "Applicant");
        var employer = TestDbContextFactory.CreateTestUser("emp5", "emp5@test.com", "Employer");
        context.Users.AddRange(applicant, employer);

        var job = TestDbContextFactory.CreateTestJob(6, "Job 6", employer.Id);
        job.Employer = employer;
        context.Jobs.Add(job);
        context.Applications.AddRange(
            new Application { JobId = 6, Job = job, ApplicantId = applicant.Id, Applicant = applicant, Status = ApplicationStatus.Pending, AppliedDate = DateTime.UtcNow },
            new Application { JobId = 6, Job = job, ApplicantId = applicant.Id, Applicant = applicant, Status = ApplicationStatus.Pending, AppliedDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var count = await repo.GetCountAsync();

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetByJobIdsAsync_ReturnsMatching()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new ApplicationRepository(context);
        var applicant = TestDbContextFactory.CreateTestUser("app6", "app6@test.com", "Applicant");
        var employer = TestDbContextFactory.CreateTestUser("emp6", "emp6@test.com", "Employer");
        context.Users.AddRange(applicant, employer);

        var job1 = TestDbContextFactory.CreateTestJob(7, "Job 7", employer.Id);
        job1.Employer = employer;
        var job2 = TestDbContextFactory.CreateTestJob(8, "Job 8", employer.Id);
        job2.Employer = employer;
        context.Jobs.AddRange(job1, job2);

        context.Applications.AddRange(
            new Application { JobId = 7, Job = job1, ApplicantId = applicant.Id, Applicant = applicant, Status = ApplicationStatus.Pending, AppliedDate = DateTime.UtcNow },
            new Application { JobId = 8, Job = job2, ApplicantId = applicant.Id, Applicant = applicant, Status = ApplicationStatus.Pending, AppliedDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var apps = await repo.GetByJobIdsAsync(new List<int> { 7, 8 });

        Assert.Equal(2, apps.Count);
    }

    [Fact]
    public async Task GetJobApplications_ReturnsForJob()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new ApplicationRepository(context);
        var applicant = TestDbContextFactory.CreateTestUser("app7", "app7@test.com", "Applicant");
        var employer = TestDbContextFactory.CreateTestUser("emp7", "emp7@test.com", "Employer");
        context.Users.AddRange(applicant, employer);

        var job = TestDbContextFactory.CreateTestJob(9, "Job 9", employer.Id);
        job.Employer = employer;
        context.Jobs.Add(job);

        context.Applications.Add(
            new Application { JobId = 9, Job = job, ApplicantId = applicant.Id, Applicant = applicant, Status = ApplicationStatus.Pending, AppliedDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var apps = await repo.GetJobApplications(9);

        Assert.Single(apps);
    }
}
