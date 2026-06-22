using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Controllers;

namespace JobFinderNet.Tests.Controllers;

public class ProfileControllerTests
{
    private readonly Mock<IUserProfileService> _mockService;
    private readonly ProfileController _controller;

    public ProfileControllerTests()
    {
        _mockService = new Mock<IUserProfileService>();
        _controller = new ProfileController(_mockService.Object);
        SetUser("test-user-id");
    }

    private void SetUser(string userId)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")) }
        };
    }

    [Fact]
    public async Task GetProfile_ReturnsOkWithDto()
    {
        var profile = new UserProfile { Id = 1, UserId = "test-user-id", Skills = ["C#"], EmailFrequency = "daily", MinimumMatchScore = 20, IsOpenToRemote = true };
        _mockService.Setup(s => s.GetOrCreateProfileAsync("test-user-id")).ReturnsAsync(profile);

        var result = await _controller.GetProfile();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<UserProfileDto>(okResult.Value);
        Assert.Equal("test-user-id", dto.UserId);
        Assert.Contains("C#", dto.Skills);
    }

    [Fact]
    public async Task UpdateProfile_ReturnsOkWithDto()
    {
        var profile = new UserProfile { Id = 1, UserId = "test-user-id", Skills = ["C#", "React"], MinimumMatchScore = 80 };
        var dto = new UpdateProfileDto { Skills = ["C#", "React"], MinimumMatchScore = 80 };
        _mockService.Setup(s => s.UpdateProfileAsync("test-user-id", dto)).ReturnsAsync(profile);

        var result = await _controller.UpdateProfile(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<UserProfileDto>(okResult.Value);
        Assert.Equal(2, returned.Skills.Count);
        Assert.Equal(80, returned.MinimumMatchScore);
    }

    [Fact]
    public async Task GetMatchedJobs_ReturnsOk()
    {
        _mockService.Setup(s => s.GetMatchedJobsAsync("test-user-id", 6))
            .ReturnsAsync(new List<object> { new { Id = 1, Title = "Dev" } });

        var result = await _controller.GetMatchedJobs();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetMatchedJobsDetailed_ReturnsOk()
    {
        _mockService.Setup(s => s.GetMatchedJobsDetailedAsync("test-user-id", 12))
            .ReturnsAsync(new List<MatchedJobDto> { new() { Id = 1, Title = "Dev" } });

        var result = await _controller.GetMatchedJobsDetailed();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAvailableSkills_ReturnsOk()
    {
        _mockService.Setup(s => s.GetAvailableSkillsAsync())
            .ReturnsAsync(new List<string> { "C#", "React" });

        var result = await _controller.GetAvailableSkills();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var skills = Assert.IsType<List<string>>(okResult.Value);
        Assert.Equal(2, skills.Count);
    }
}
