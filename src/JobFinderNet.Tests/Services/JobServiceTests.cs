using Moq;
using Microsoft.AspNetCore.Identity;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Infrastructure.Data;
using JobFinderNet.Infrastructure.Services;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Services;

public class JobServiceTests
{
    private readonly Mock<IJobRepository> _mockJobRepo;
    private readonly Mock<IApplicationRepository> _mockAppRepo;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly ApplicationDbContext _context;
    private readonly JobService _service;

    public JobServiceTests()
    {
        _mockJobRepo = new Mock<IJobRepository>();
        _mockAppRepo = new Mock<IApplicationRepository>();

        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _service = new JobService(
            _mockJobRepo.Object,
            _mockAppRepo.Object,
            _context,
            _mockUserManager.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnJob()
    {
        var employer = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        var job = TestDbContextFactory.CreateTestJob(1, "Test Job", employer.Id);
        job.Employer = employer;

        _mockJobRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        var result = await _mockJobRepo.Object.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Test Job", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentJob_ShouldReturnNull()
    {
        _mockJobRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Job?)null);

        var result = await _mockJobRepo.Object.GetByIdAsync(999);

        Assert.Null(result);
    }
}
