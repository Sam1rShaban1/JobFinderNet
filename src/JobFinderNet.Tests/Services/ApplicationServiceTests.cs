using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Data;
using JobFinderNet.Infrastructure.Services;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Services;

public class ApplicationServiceTests
{
    private readonly Mock<IJobRepository> _mockJobRepo;
    private readonly Mock<IApplicationRepository> _mockAppRepo;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly ApplicationDbContext _context;
    private readonly ApplicationService _service;

    public ApplicationServiceTests()
    {
        _mockJobRepo = new Mock<IJobRepository>();
        _mockAppRepo = new Mock<IApplicationRepository>();

        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockNotificationService = new Mock<INotificationService>();
        _context = TestDbContextFactory.CreateInMemoryDbContext();

        _service = new ApplicationService(
            _mockJobRepo.Object,
            _mockAppRepo.Object,
            _mockUserManager.Object,
            _context,
            _mockNotificationService.Object);
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
        _mockNotificationService.Verify(
            n => n.SendApplicationSubmittedAsync("app@test.com", "app@test.com", "Test Job", "Test Corp"),
            Times.Once);
        _mockNotificationService.Verify(
            n => n.SendNewApplicationToEmployerAsync("emp@test.com", "Test Company", "app@test.com", "Test Job"),
            Times.Once);
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
}
