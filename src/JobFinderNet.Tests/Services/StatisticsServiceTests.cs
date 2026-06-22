using Moq;
using Microsoft.AspNetCore.Identity;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Infrastructure.Services;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Services;

public class StatisticsServiceTests
{
    private readonly Mock<IJobRepository> _mockJobRepo;
    private readonly Mock<IApplicationRepository> _mockAppRepo;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly StatisticsService _service;

    public StatisticsServiceTests()
    {
        _mockJobRepo = new Mock<IJobRepository>();
        _mockAppRepo = new Mock<IApplicationRepository>();

        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _service = new StatisticsService(_mockJobRepo.Object, _mockAppRepo.Object, _mockUserManager.Object);
    }

    [Fact]
    public async Task GetPublicStatisticsAsync_ReturnsAggregatedStats()
    {
        var jobs = new List<Job>
        {
            TestDbContextFactory.CreateTestJob(1, "Job1", "emp1"),
            TestDbContextFactory.CreateTestJob(2, "Job2", "emp1")
        };
        jobs[0].RequiredTechnologies = ["C#"];
        jobs[0].PreferredTechnologies = [];
        jobs[1].RequiredTechnologies = [];
        jobs[1].PreferredTechnologies = ["React"];

        _mockJobRepo.Setup(r => r.GetCountAsync()).ReturnsAsync(2);
        _mockJobRepo.Setup(r => r.GetJobsByTypeAsync()).ReturnsAsync(new Dictionary<string, int> { { "Full-time", 2 }, { "Contract", 1 } });
        _mockJobRepo.Setup(r => r.GetAllActiveJobsAsync()).ReturnsAsync(jobs);
        _mockAppRepo.Setup(r => r.GetCountAsync()).ReturnsAsync(5);
        _mockUserManager.Setup(u => u.GetUsersInRoleAsync("Applicant")).ReturnsAsync(new List<ApplicationUser> { new() { Id = "a1" } });
        _mockUserManager.Setup(u => u.GetUsersInRoleAsync("Employer")).ReturnsAsync(new List<ApplicationUser> { new() { Id = "e1" } });

        var result = await _service.GetPublicStatisticsAsync();

        Assert.Equal(2, result.TotalJobs);
        Assert.Equal(5, result.TotalApplications);
        Assert.Equal(2, result.TotalUsers);
        Assert.Equal(2, result.TotalTechnologies);
        Assert.Equal(2, result.JobsByType.Count);
    }

    [Fact]
    public async Task GetEmployerDashboardAsync_ReturnsEmployerStats()
    {
        var employerJobs = new List<Job>
        {
            TestDbContextFactory.CreateTestJob(1, "Job1", "emp1"),
            TestDbContextFactory.CreateTestJob(2, "Job2", "emp1")
        };
        employerJobs[1].IsActive = false;

        var applications = new List<Application>
        {
            TestDbContextFactory.CreateTestApplication(1, 1, "app1"),
            TestDbContextFactory.CreateTestApplication(2, 1, "app2")
        };
        applications[0].Status = ApplicationStatus.Pending;
        applications[1].Status = ApplicationStatus.Accepted;

        _mockJobRepo.Setup(r => r.GetEmployerJobsAsync("emp1")).ReturnsAsync(employerJobs);
        _mockAppRepo.Setup(r => r.GetByJobIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(applications);

        var result = await _service.GetEmployerDashboardAsync("emp1");

        Assert.Equal(2, result.TotalJobs);
        Assert.Equal(1, result.ActiveJobs);
        Assert.Equal(2, result.TotalApplications);
        Assert.Equal(2, result.ApplicationsByStatus.Count);
        Assert.Equal(2, result.TopJobs.Count);
        Assert.Equal(2, result.TopJobs[0].ApplicationCount);
    }

    [Fact]
    public async Task GetEmployerJobCountAsync_ReturnsCount()
    {
        var jobs = new List<Job> { TestDbContextFactory.CreateTestJob(1, "Job1", "emp1") };
        _mockJobRepo.Setup(r => r.GetEmployerJobsAsync("emp1")).ReturnsAsync(jobs);

        var result = await _service.GetEmployerJobCountAsync("emp1");

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetApplicationsByJobAsync_ReturnsDictionary()
    {
        var jobs = new List<Job> { TestDbContextFactory.CreateTestJob(1, "Job1", "emp1") };
        var applications = new List<Application> { TestDbContextFactory.CreateTestApplication(1, 1, "app1") };

        _mockJobRepo.Setup(r => r.GetEmployerJobsAsync("emp1")).ReturnsAsync(jobs);
        _mockAppRepo.Setup(r => r.GetByJobIdsAsync(new List<int> { 1 })).ReturnsAsync(applications);

        var result = await _service.GetApplicationsByJobAsync("emp1");

        Assert.Single(result);
        Assert.Equal(1, result["Job1"]);
    }
}
