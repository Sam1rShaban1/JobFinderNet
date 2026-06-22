using Moq;
using Microsoft.AspNetCore.Identity;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Infrastructure.Services;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Services;

public class JobServiceTests
{
    private readonly Mock<IJobRepository> _mockJobRepo;
    private readonly Mock<IApplicationRepository> _mockAppRepo;
    private readonly Mock<ICompanyProfileRepository> _mockCompanyProfileRepo;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly JobService _service;

    public JobServiceTests()
    {
        _mockJobRepo = new Mock<IJobRepository>();
        _mockAppRepo = new Mock<IApplicationRepository>();
        _mockCompanyProfileRepo = new Mock<ICompanyProfileRepository>();

        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _service = new JobService(
            _mockJobRepo.Object,
            _mockAppRepo.Object,
            _mockCompanyProfileRepo.Object,
            _mockUserManager.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnJob()
    {
        var employer = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        var job = TestDbContextFactory.CreateTestJob(1, "Test Job", employer.Id);
        job.Employer = employer;

        _mockJobRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Test Job", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentJob_ShouldReturnNull()
    {
        _mockJobRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Job?)null);

        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPaginatedJobsAsync_ReturnsPaginatedList()
    {
        var paginatedList = new PaginatedList<Job>(new List<Job>(), 0, 1, 12);
        _mockJobRepo.Setup(r => r.GetPaginatedJobsAsync(1, 12)).ReturnsAsync(paginatedList);

        var result = await _service.GetPaginatedJobsAsync(1, 12);

        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task SearchJobsAsync_ReturnsResults()
    {
        var jobs = new List<Job> { TestDbContextFactory.CreateTestJob(1, "Software Engineer", "emp1") };
        _mockJobRepo.Setup(r => r.SearchJobsAsync("Engineer")).ReturnsAsync(jobs);

        var result = await _service.SearchJobsAsync("Engineer");

        Assert.Single(result);
    }

    [Fact]
    public async Task GetEmployerJobsAsync_ReturnsJobs()
    {
        var jobs = new List<Job>
        {
            TestDbContextFactory.CreateTestJob(1, "Job1", "emp1"),
            TestDbContextFactory.CreateTestJob(2, "Job2", "emp1")
        };
        _mockJobRepo.Setup(r => r.GetEmployerJobsAsync("emp1")).ReturnsAsync(jobs);

        var result = await _service.GetEmployerJobsAsync("emp1");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CreateJobAsync_ValidData_ReturnsJob()
    {
        var employer = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        var dto = new CreateJobDto
        {
            Title = "New Job",
            Description = "Description",
            CompanyName = "Test Corp",
            Location = "Remote",
            JobType = "Full-time",
            Salary = "$80k",
            ExperienceRequired = "Entry"
        };

        _mockUserManager.Setup(u => u.FindByIdAsync("emp1")).ReturnsAsync(employer);
        _mockCompanyProfileRepo.Setup(r => r.GetByClaimedUserIdAsync("emp1")).ReturnsAsync((CompanyProfile?)null);
        _mockJobRepo.Setup(r => r.CreateJobAsync(It.IsAny<Job>()));

        var result = await _service.CreateJobAsync(dto, "emp1");

        Assert.NotNull(result);
        Assert.Equal("New Job", result.Title);
        Assert.Equal("emp1", result.EmployerId);
    }

    [Fact]
    public async Task CreateJobAsync_UserNotFound_Throws()
    {
        var dto = new CreateJobDto
        {
            Title = "New Job", Description = "Desc", CompanyName = "C", Location = "L",
            JobType = "Full-time", Salary = "$80k", ExperienceRequired = "Entry"
        };
        _mockUserManager.Setup(u => u.FindByIdAsync("bad")).ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateJobAsync(dto, "bad"));
    }

    [Fact]
    public async Task UpdateJobAsync_ValidData_ReturnsUpdatedJob()
    {
        var employer = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        var existingJob = TestDbContextFactory.CreateTestJob(1, "Old Title", employer.Id);
        existingJob.Employer = employer;
        var dto = new CreateJobDto
        {
            Title = "Updated Title", Description = "Updated Desc", CompanyName = "Test Corp",
            Location = "Remote", JobType = "Full-time", Salary = "$90k", ExperienceRequired = "Senior"
        };

        _mockJobRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingJob);
        _mockCompanyProfileRepo.Setup(r => r.GetByClaimedUserIdAsync("emp1")).ReturnsAsync((CompanyProfile?)null);

        var result = await _service.UpdateJobAsync(1, dto, "emp1");

        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
    }

    [Fact]
    public async Task UpdateJobAsync_NotOwner_ReturnsNull()
    {
        var job = TestDbContextFactory.CreateTestJob(1, "Job", "emp1");
        var dto = new CreateJobDto
        {
            Title = "Updated", Description = "Desc", CompanyName = "C", Location = "L",
            JobType = "FT", Salary = "$80k", ExperienceRequired = "Entry"
        };
        _mockJobRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        var result = await _service.UpdateJobAsync(1, dto, "emp2");

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteJobAsync_Valid_Deletes()
    {
        var job = TestDbContextFactory.CreateTestJob(1, "Job", "emp1");
        _mockJobRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        await _service.DeleteJobAsync(1, "emp1");

        _mockJobRepo.Verify(r => r.DeleteJobAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteJobAsync_NotFound_Throws()
    {
        _mockJobRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Job?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteJobAsync(999, "emp1"));
    }

    [Fact]
    public async Task DeleteJobAsync_NotOwner_Throws()
    {
        var job = TestDbContextFactory.CreateTestJob(1, "Job", "emp1");
        _mockJobRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteJobAsync(1, "emp2"));
    }

    [Fact]
    public async Task ToggleJobStatusAsync_Valid_Toggles()
    {
        var job = TestDbContextFactory.CreateTestJob(1, "Job", "emp1");
        _mockJobRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        await _service.ToggleJobStatusAsync(1, "emp1");

        _mockJobRepo.Verify(r => r.ToggleJobStatusAsync(1), Times.Once);
    }

    [Fact]
    public async Task ToggleJobStatusAsync_NotFound_Throws()
    {
        _mockJobRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Job?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ToggleJobStatusAsync(999, "emp1"));
    }

    [Fact]
    public async Task ToggleJobStatusAsync_NotOwner_Throws()
    {
        var job = TestDbContextFactory.CreateTestJob(1, "Job", "emp1");
        _mockJobRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ToggleJobStatusAsync(1, "emp2"));
    }

    [Fact]
    public async Task GetSimilarJobsAsync_ReturnsJobs()
    {
        var job = TestDbContextFactory.CreateTestJob(1, "Job", "emp1");
        var similar = new List<Job> { TestDbContextFactory.CreateTestJob(2, "Similar Job", "emp1") };
        _mockJobRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);
        _mockJobRepo.Setup(r => r.GetSimilarJobsAsync(1, job.Industry, job.CompanyName, job.RequiredTechnologies))
            .ReturnsAsync(similar);

        var result = await _service.GetSimilarJobsAsync(1);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetSimilarJobsAsync_JobNotFound_ReturnsEmpty()
    {
        _mockJobRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Job?)null);

        var result = await _service.GetSimilarJobsAsync(999);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetJobApplicationsAsync_Valid_ReturnsApplications()
    {
        var employer = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        var job = TestDbContextFactory.CreateTestJob(1, "Job", employer.Id);
        job.Employer = employer;
        var apps = new List<Application>
        {
            TestDbContextFactory.CreateTestApplication(1, 1, "app1"),
            TestDbContextFactory.CreateTestApplication(2, 1, "app2")
        };

        _mockJobRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);
        _mockAppRepo.Setup(r => r.GetJobApplications(1)).ReturnsAsync(apps);

        var result = await _service.GetJobApplicationsAsync(1, "emp1");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetJobApplicationsAsync_NotOwner_Throws()
    {
        var job = TestDbContextFactory.CreateTestJob(1, "Job", "emp1");
        _mockJobRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetJobApplicationsAsync(1, "emp2"));
    }
}
