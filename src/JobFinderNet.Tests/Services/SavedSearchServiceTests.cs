using Moq;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Services;

namespace JobFinderNet.Tests.Services;

public class SavedSearchServiceTests
{
    private readonly Mock<ISavedSearchRepository> _mockRepo;
    private readonly Mock<IMatchingService> _mockMatching;
    private readonly SavedSearchService _service;

    public SavedSearchServiceTests()
    {
        _mockRepo = new Mock<ISavedSearchRepository>();
        _mockMatching = new Mock<IMatchingService>();
        _service = new SavedSearchService(_mockRepo.Object, _mockMatching.Object);
    }

    [Fact]
    public async Task GetUserSavedSearchesAsync_ReturnsSearches()
    {
        var searches = new List<SavedSearch> { new() { Id = 1, UserId = "u1", Name = "My Search" } };
        _mockRepo.Setup(r => r.GetUserSavedSearchesAsync("u1")).ReturnsAsync(searches);

        var result = await _service.GetUserSavedSearchesAsync("u1");

        Assert.Single(result);
        Assert.Equal("My Search", result[0].Name);
    }

    [Fact]
    public async Task CreateSavedSearchAsync_WithProfile_CreatesSearch()
    {
        var profile = new UserProfile { Id = 1, UserId = "u1", Skills = ["C#"] };
        _mockRepo.Setup(r => r.GetUserProfileAsync("u1")).ReturnsAsync(profile);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new SavedSearchDto { Name = "Test", Search = "developer", EmailFrequency = "daily" };

        var result = await _service.CreateSavedSearchAsync("u1", dto);

        Assert.Equal("Test", result.Name);
        Assert.Equal("u1", result.UserId);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<SavedSearch>()), Times.Once);
    }

    [Fact]
    public async Task CreateSavedSearchAsync_NoProfile_ThrowsInvalidOperation()
    {
        _mockRepo.Setup(r => r.GetUserProfileAsync("u1")).ReturnsAsync((UserProfile?)null);

        var dto = new SavedSearchDto { Name = "Test" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateSavedSearchAsync("u1", dto));
    }

    [Fact]
    public async Task UpdateSavedSearchAsync_Existing_UpdatesAndReturns()
    {
        var existing = new SavedSearch { Id = 1, UserId = "u1", Name = "Old" };
        _mockRepo.Setup(r => r.GetByIdForUserAsync(1, "u1")).ReturnsAsync(existing);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new SavedSearchDto { Name = "New", EmailFrequency = "weekly" };

        var result = await _service.UpdateSavedSearchAsync(1, "u1", dto);

        Assert.NotNull(result);
        Assert.Equal("New", result!.Name);
    }

    [Fact]
    public async Task UpdateSavedSearchAsync_NotFound_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByIdForUserAsync(99, "u1")).ReturnsAsync((SavedSearch?)null);

        var result = await _service.UpdateSavedSearchAsync(99, "u1", new SavedSearchDto { Name = "X" });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteSavedSearchAsync_Existing_Removes()
    {
        var existing = new SavedSearch { Id = 1, UserId = "u1", Name = "Delete Me" };
        _mockRepo.Setup(r => r.GetByIdForUserAsync(1, "u1")).ReturnsAsync(existing);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _service.DeleteSavedSearchAsync(1, "u1");

        _mockRepo.Verify(r => r.Remove(existing), Times.Once);
    }

    [Fact]
    public async Task DeleteSavedSearchAsync_NotFound_ThrowsKeyNotFound()
    {
        _mockRepo.Setup(r => r.GetByIdForUserAsync(99, "u1")).ReturnsAsync((SavedSearch?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteSavedSearchAsync(99, "u1"));
    }

    [Fact]
    public async Task RunSavedSearchAsync_ReturnsMatches()
    {
        var search = new SavedSearch { Id = 1, UserId = "u1", Name = "Search" };
        var profile = new UserProfile { Id = 1, UserId = "u1", Skills = ["C#"] };
        var job = new Job { Id = 1, Title = "Dev", Description = "D", CompanyName = "C", Location = "L", JobType = "F", Salary = "$1", ExperienceRequired = "E", EmployerId = "emp1" };
        var matches = new List<(Job Job, int Score)> { (job, 85) };

        _mockRepo.Setup(r => r.GetByIdForUserAsync(1, "u1")).ReturnsAsync(search);
        _mockRepo.Setup(r => r.GetUserProfileAsync("u1")).ReturnsAsync(profile);
        _mockMatching.Setup(m => m.GetTopMatches(profile, 10)).ReturnsAsync(matches);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.RunSavedSearchAsync(1, "u1");

        Assert.NotNull(result);
        Assert.NotNull(search.LastRunAt);
    }

    [Fact]
    public async Task RunSavedSearchAsync_NotFound_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByIdForUserAsync(99, "u1")).ReturnsAsync((SavedSearch?)null);

        var result = await _service.RunSavedSearchAsync(99, "u1");

        Assert.Null(result);
    }
}
