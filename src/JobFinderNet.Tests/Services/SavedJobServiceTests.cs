using Moq;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Infrastructure.Services;

namespace JobFinderNet.Tests.Services;

public class SavedJobServiceTests
{
    private readonly Mock<ISavedJobRepository> _mockSavedJobRepo;
    private readonly Mock<IJobRepository> _mockJobRepo;
    private readonly SavedJobService _service;

    public SavedJobServiceTests()
    {
        _mockSavedJobRepo = new Mock<ISavedJobRepository>();
        _mockJobRepo = new Mock<IJobRepository>();
        _service = new SavedJobService(_mockSavedJobRepo.Object, _mockJobRepo.Object);
    }

    [Fact]
    public async Task GetUserSavedJobsAsync_ReturnsSavedJobs()
    {
        var saved = new List<SavedJob> { new() { UserId = "u1", JobId = 1, SavedDate = DateTime.UtcNow } };
        _mockSavedJobRepo.Setup(r => r.GetUserSavedJobsAsync("u1")).ReturnsAsync(saved);

        var result = await _service.GetUserSavedJobsAsync("u1");

        Assert.Single(result);
    }

    [Fact]
    public async Task GetUserSavedJobIdsAsync_ReturnsIds()
    {
        _mockSavedJobRepo.Setup(r => r.GetUserSavedJobIdsAsync("u1")).ReturnsAsync(new List<int> { 1, 2, 3 });

        var result = await _service.GetUserSavedJobIdsAsync("u1");

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task SaveJobAsync_NewJob_AddsAndSaves()
    {
        var job = new Job { Id = 1, Title = "Test", Description = "D", CompanyName = "C", Location = "L", JobType = "F", Salary = "$1", ExperienceRequired = "E", EmployerId = "emp1" };
        _mockJobRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);
        _mockSavedJobRepo.Setup(r => r.GetAsync("u1", 1)).ReturnsAsync((SavedJob?)null);
        _mockSavedJobRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _service.SaveJobAsync("u1", 1);

        _mockSavedJobRepo.Verify(r => r.AddAsync(It.Is<SavedJob>(s => s.UserId == "u1" && s.JobId == 1)), Times.Once);
        _mockSavedJobRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SaveJobAsync_AlreadySaved_DoesNotAddAgain()
    {
        var job = new Job { Id = 1, Title = "Test", Description = "D", CompanyName = "C", Location = "L", JobType = "F", Salary = "$1", ExperienceRequired = "E", EmployerId = "emp1" };
        var existing = new SavedJob { UserId = "u1", JobId = 1 };
        _mockJobRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);
        _mockSavedJobRepo.Setup(r => r.GetAsync("u1", 1)).ReturnsAsync(existing);

        await _service.SaveJobAsync("u1", 1);

        _mockSavedJobRepo.Verify(r => r.AddAsync(It.IsAny<SavedJob>()), Times.Never);
    }

    [Fact]
    public async Task SaveJobAsync_JobNotFound_ThrowsKeyNotFoundException()
    {
        _mockJobRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Job?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.SaveJobAsync("u1", 99));
    }

    [Fact]
    public async Task UnsaveJobAsync_ExistingJob_Removes()
    {
        var existing = new SavedJob { UserId = "u1", JobId = 1 };
        _mockSavedJobRepo.Setup(r => r.GetAsync("u1", 1)).ReturnsAsync(existing);
        _mockSavedJobRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _service.UnsaveJobAsync("u1", 1);

        _mockSavedJobRepo.Verify(r => r.Remove(existing), Times.Once);
        _mockSavedJobRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UnsaveJobAsync_NotFound_ThrowsKeyNotFoundException()
    {
        _mockSavedJobRepo.Setup(r => r.GetAsync("u1", 99)).ReturnsAsync((SavedJob?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UnsaveJobAsync("u1", 99));
    }
}
