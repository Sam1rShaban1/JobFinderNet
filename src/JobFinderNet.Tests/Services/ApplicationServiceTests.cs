using Moq;
using Microsoft.AspNetCore.Identity;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Infrastructure.Services;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Services;

public class ApplicationServiceTests
{
    private readonly Mock<IJobRepository> _mockJobRepo;
    private readonly Mock<IApplicationRepository> _mockAppRepo;
    private readonly Mock<IApplicationNoteRepository> _mockAppNoteRepo;
    private readonly Mock<INotificationRepository> _mockNotificationRepo;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly ApplicationService _service;

    public ApplicationServiceTests()
    {
        _mockJobRepo = new Mock<IJobRepository>();
        _mockAppRepo = new Mock<IApplicationRepository>();
        _mockAppNoteRepo = new Mock<IApplicationNoteRepository>();
        _mockNotificationRepo = new Mock<INotificationRepository>();

        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _service = new ApplicationService(
            _mockJobRepo.Object,
            _mockAppRepo.Object,
            _mockAppNoteRepo.Object,
            _mockNotificationRepo.Object,
            _mockUserManager.Object);
    }

    [Fact]
    public async Task SubmitApplicationAsync_WithValidData_ShouldSucceed()
    {
        var employer = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        var applicant = TestDbContextFactory.CreateTestUser("app1", "app@test.com", "Applicant");
        var job = TestDbContextFactory.CreateTestJob(1, "Test Job", employer.Id);
        job.Employer = employer;

        _mockJobRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);
        _mockAppRepo.Setup(r => r.HasUserAppliedToJob("app1", 1)).ReturnsAsync(false);
        _mockAppRepo.Setup(r => r.AddAsync(It.IsAny<Application>())).ReturnsAsync(true);
        _mockUserManager.Setup(u => u.FindByIdAsync("app1")).ReturnsAsync(applicant);

        var result = await _service.SubmitApplicationAsync(1, "app1");

        Assert.True(result.Succeeded);
        Assert.Equal("Application submitted successfully", result.Message);
    }

    [Fact]
    public async Task SubmitApplicationAsync_ToInactiveJob_ShouldFail()
    {
        var employer = TestDbContextFactory.CreateTestUser("emp2", "emp@test.com", "Employer");
        var job = TestDbContextFactory.CreateTestJob(2, "Inactive Job", employer.Id);
        job.IsActive = false;
        job.Employer = employer;

        _mockJobRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(job);

        var result = await _service.SubmitApplicationAsync(2, "app2");

        Assert.False(result.Succeeded);
        Assert.Contains("no longer accepting", result.Message.ToLower());
    }

    [Fact]
    public async Task SubmitApplicationAsync_Duplicate_ShouldFail()
    {
        var employer = TestDbContextFactory.CreateTestUser("emp3", "emp@test.com", "Employer");
        var job = TestDbContextFactory.CreateTestJob(3, "Job", employer.Id);
        job.Employer = employer;

        _mockJobRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(job);
        _mockAppRepo.Setup(r => r.HasUserAppliedToJob("app3", 3)).ReturnsAsync(true);

        var result = await _service.SubmitApplicationAsync(3, "app3");

        Assert.False(result.Succeeded);
        Assert.Contains("already applied", result.Message.ToLower());
    }

    [Fact]
    public async Task SubmitApplicationAsync_OwnJob_ShouldFail()
    {
        var employer = TestDbContextFactory.CreateTestUser("emp4", "emp@test.com", "Employer");
        var job = TestDbContextFactory.CreateTestJob(4, "Own Job", employer.Id);
        job.Employer = employer;

        _mockJobRepo.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(job);

        var result = await _service.SubmitApplicationAsync(4, employer.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("employers cannot apply", result.Message.ToLower());
    }

    [Fact]
    public async Task GetUserApplicationsAsync_ReturnsApplications()
    {
        var apps = new List<Application>
        {
            TestDbContextFactory.CreateTestApplication(1, 1, "app1"),
            TestDbContextFactory.CreateTestApplication(2, 1, "app1")
        };
        _mockAppRepo.Setup(r => r.GetUserApplicationsAsync("app1")).ReturnsAsync(apps);

        var result = await _service.GetUserApplicationsAsync("app1");

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_Valid_ReturnsSuccess()
    {
        var employer = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        var app = TestDbContextFactory.CreateTestApplication(1, 1, "app1");
        _mockAppRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(app);
        _mockAppRepo.Setup(r => r.AddAsync(It.IsAny<Application>())).ReturnsAsync(true);
        _mockNotificationRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.UpdateApplicationStatusAsync(1, ApplicationStatus.Accepted);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_NotFound_ReturnsError()
    {
        _mockAppRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Application?)null);

        var result = await _service.UpdateApplicationStatusAsync(999, ApplicationStatus.Accepted);

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Message.ToLower());
    }

    [Fact]
    public async Task GetNotesAsync_ReturnsNotes()
    {
        var notes = new List<ApplicationNote>
        {
            new() { Id = 1, ApplicationId = 1, UserId = "emp1", Content = "Note 1", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, ApplicationId = 1, UserId = "emp1", Content = "Note 2", CreatedAt = DateTime.UtcNow }
        };
        _mockAppNoteRepo.Setup(r => r.GetByApplicationIdAsync(1)).ReturnsAsync(notes);

        var result = await _service.GetNotesAsync(1);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task AddNoteAsync_Valid_ReturnsNote()
    {
        var app = TestDbContextFactory.CreateTestApplication(1, 1, "app1");
        _mockAppRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(app);

        var result = await _service.AddNoteAsync(1, "emp1", "Test note");

        Assert.NotNull(result);
        Assert.Equal("Test note", result.Content);
        Assert.Equal("emp1", result.UserId);
    }

    [Fact]
    public async Task AddNoteAsync_AppNotFound_ReturnsNull()
    {
        _mockAppRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Application?)null);

        var result = await _service.AddNoteAsync(999, "emp1", "Test note");

        Assert.Null(result);
    }

    [Fact]
    public async Task HasUserAppliedAsync_ReturnsTrue()
    {
        _mockAppRepo.Setup(r => r.HasUserAppliedToJob("app1", 1)).ReturnsAsync(true);

        var result = await _service.HasUserAppliedAsync("app1", 1);

        Assert.True(result);
    }

    [Fact]
    public async Task HasUserAppliedAsync_ReturnsFalse()
    {
        _mockAppRepo.Setup(r => r.HasUserAppliedToJob("app1", 1)).ReturnsAsync(false);

        var result = await _service.HasUserAppliedAsync("app1", 1);

        Assert.False(result);
    }

    [Fact]
    public async Task GetJobApplicationsAsync_ReturnsApplications()
    {
        var apps = new List<Application>
        {
            TestDbContextFactory.CreateTestApplication(1, 1, "app1")
        };
        _mockAppRepo.Setup(r => r.GetJobApplications(1)).ReturnsAsync(apps);

        var result = await _service.GetJobApplicationsAsync(1);

        Assert.Single(result);
    }
}
