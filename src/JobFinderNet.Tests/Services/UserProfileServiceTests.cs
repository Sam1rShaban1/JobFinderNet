using Moq;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Services;

namespace JobFinderNet.Tests.Services;

public class UserProfileServiceTests
{
    private readonly Mock<IUserProfileRepository> _mockRepo;
    private readonly Mock<IMatchingService> _mockMatching;
    private readonly UserProfileService _service;

    public UserProfileServiceTests()
    {
        _mockRepo = new Mock<IUserProfileRepository>();
        _mockMatching = new Mock<IMatchingService>();
        _service = new UserProfileService(_mockRepo.Object, _mockMatching.Object);
    }

    [Fact]
    public async Task GetOrCreateProfileAsync_ExistingProfile_ReturnsIt()
    {
        var profile = new UserProfile { Id = 1, UserId = "u1", Skills = ["C#"] };
        _mockRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(profile);

        var result = await _service.GetOrCreateProfileAsync("u1");

        Assert.Equal(1, result.Id);
        Assert.Contains("C#", result.Skills);
    }

    [Fact]
    public async Task GetOrCreateProfileAsync_NoProfile_CreatesNew()
    {
        _mockRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync((UserProfile?)null);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<UserProfile>())).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.GetOrCreateProfileAsync("u1");

        Assert.Equal("u1", result.UserId);
        Assert.True(result.IsOpenToRemote);
        Assert.Equal("daily", result.EmailFrequency);
    }

    [Fact]
    public async Task UpdateProfileAsync_ExistingProfile_UpdatesFields()
    {
        var profile = new UserProfile { Id = 1, UserId = "u1", Skills = ["C#"], MinimumMatchScore = 20 };
        _mockRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(profile);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new UpdateProfileDto
        {
            Skills = ["C#", "React"],
            SeniorityLevel = "Senior",
            DesiredSalaryMin = 100000,
            DesiredSalaryMax = 150000,
            IsOpenToRemote = false,
            PreferredLocation = "NYC",
            PreferredJobType = "Full-time",
            EmailOnMatch = false,
            MinimumMatchScore = 80,
            EmailFrequency = "weekly"
        };

        var result = await _service.UpdateProfileAsync("u1", dto);

        Assert.Equal(2, result.Skills.Count);
        Assert.Equal("Senior", result.SeniorityLevel);
        Assert.Equal(100000, result.DesiredSalaryMin);
        Assert.Equal(150000, result.DesiredSalaryMax);
        Assert.False(result.IsOpenToRemote);
        Assert.Equal("NYC", result.PreferredLocation);
        Assert.Equal("Full-time", result.PreferredJobType);
        Assert.False(result.EmailOnMatch);
        Assert.Equal(80, result.MinimumMatchScore);
        Assert.Equal("weekly", result.EmailFrequency);
    }

    [Fact]
    public async Task UpdateProfileAsync_NoProfile_CreatesAndUpdates()
    {
        _mockRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync((UserProfile?)null);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<UserProfile>())).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new UpdateProfileDto { Skills = ["Python"], MinimumMatchScore = 50 };

        var result = await _service.UpdateProfileAsync("u1", dto);

        Assert.Equal("u1", result.UserId);
        Assert.Contains("Python", result.Skills);
    }

    [Fact]
    public async Task GetMatchedJobsAsync_WithSkills_ReturnsMatches()
    {
        var profile = new UserProfile { Id = 1, UserId = "u1", Skills = ["C#"] };
        _mockRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(profile);
        _mockMatching.Setup(m => m.GetTopMatches(profile, 6)).ReturnsAsync(
            new List<(Job, int)> { (new Job { Id = 1, Title = "Dev", Description = "D", CompanyName = "C", Location = "L", JobType = "F", Salary = "$1", ExperienceRequired = "E", EmployerId = "emp1" }, 85) });

        var result = await _service.GetMatchedJobsAsync("u1", 6);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetMatchedJobsAsync_NoProfile_ReturnsEmpty()
    {
        _mockRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync((UserProfile?)null);

        var result = await _service.GetMatchedJobsAsync("u1");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMatchedJobsAsync_NoSkills_ReturnsEmpty()
    {
        var profile = new UserProfile { Id = 1, UserId = "u1", Skills = [] };
        _mockRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(profile);

        var result = await _service.GetMatchedJobsAsync("u1");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMatchedJobsDetailedAsync_WithSkills_ReturnsDetailedMatches()
    {
        var profile = new UserProfile { Id = 1, UserId = "u1", Skills = ["C#"] };
        _mockRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(profile);
        _mockMatching.Setup(m => m.GetTopMatchesDetailed(profile, 12)).ReturnsAsync(new List<MatchedJobDto>
        {
            new() { Id = 1, Title = "Dev", Score = 85 }
        });

        var result = await _service.GetMatchedJobsDetailedAsync("u1", 12);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetAvailableSkillsAsync_ReturnsListOfSkills()
    {
        var result = await _service.GetAvailableSkillsAsync();

        Assert.NotEmpty(result);
        Assert.Contains("C#", result);
        Assert.Contains("React", result);
        Assert.Contains("Python", result);
    }
}
