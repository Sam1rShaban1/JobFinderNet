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
}
