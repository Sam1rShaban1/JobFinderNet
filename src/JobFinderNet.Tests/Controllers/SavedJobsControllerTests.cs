using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Controllers;

namespace JobFinderNet.Tests.Controllers;

public class SavedJobsControllerTests
{
    private readonly Mock<ISavedJobService> _mockService;
    private readonly SavedJobsController _controller;

    public SavedJobsControllerTests()
    {
        _mockService = new Mock<ISavedJobService>();
        _controller = new SavedJobsController(_mockService.Object);
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
    public async Task GetSavedJobs_ReturnsOk()
    {
        _mockService.Setup(s => s.GetUserSavedJobsAsync("test-user-id"))
            .ReturnsAsync(new List<SavedJob> { new() { UserId = "test-user-id", JobId = 1 } });

        var result = await _controller.GetSavedJobs();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var saved = Assert.IsType<List<SavedJob>>(okResult.Value);
        Assert.Single(saved);
    }

    [Fact]
    public async Task SaveJob_ValidJob_ReturnsOk()
    {
        _mockService.Setup(s => s.SaveJobAsync("test-user-id", 1)).Returns(Task.CompletedTask);

        var result = await _controller.SaveJob(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task SaveJob_NotFound_ReturnsNotFound()
    {
        _mockService.Setup(s => s.SaveJobAsync("test-user-id", 99))
            .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.SaveJob(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UnsaveJob_ValidJob_ReturnsOk()
    {
        _mockService.Setup(s => s.UnsaveJobAsync("test-user-id", 1)).Returns(Task.CompletedTask);

        var result = await _controller.UnsaveJob(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UnsaveJob_NotFound_ReturnsNotFound()
    {
        _mockService.Setup(s => s.UnsaveJobAsync("test-user-id", 99))
            .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.UnsaveJob(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetSavedJobIds_ReturnsOk()
    {
        _mockService.Setup(s => s.GetUserSavedJobIdsAsync("test-user-id"))
            .ReturnsAsync(new List<int> { 1, 2, 3 });

        var result = await _controller.GetSavedJobIds();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var ids = Assert.IsType<List<int>>(okResult.Value);
        Assert.Equal(3, ids.Count);
    }
}
