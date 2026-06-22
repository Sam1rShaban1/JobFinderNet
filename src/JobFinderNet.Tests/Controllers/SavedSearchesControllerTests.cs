using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Controllers;

namespace JobFinderNet.Tests.Controllers;

public class SavedSearchesControllerTests
{
    private readonly Mock<ISavedSearchService> _mockService;
    private readonly SavedSearchesController _controller;

    public SavedSearchesControllerTests()
    {
        _mockService = new Mock<ISavedSearchService>();
        _controller = new SavedSearchesController(_mockService.Object);
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
    public async Task GetSavedSearches_ReturnsOk()
    {
        _mockService.Setup(s => s.GetUserSavedSearchesAsync("test-user-id"))
            .ReturnsAsync(new List<SavedSearch> { new() { Id = 1, UserId = "test-user-id", Name = "Search" } });

        var result = await _controller.GetSavedSearches();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var searches = Assert.IsType<List<SavedSearch>>(okResult.Value);
        Assert.Single(searches);
    }

    [Fact]
    public async Task CreateSavedSearch_ValidDto_ReturnsOk()
    {
        var dto = new SavedSearchDto { Name = "Test", EmailFrequency = "daily" };
        _mockService.Setup(s => s.CreateSavedSearchAsync("test-user-id", dto))
            .ReturnsAsync(new SavedSearch { Id = 1, UserId = "test-user-id", Name = "Test" });

        var result = await _controller.CreateSavedSearch(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateSavedSearch_NoProfile_ReturnsBadRequest()
    {
        var dto = new SavedSearchDto { Name = "Test" };
        _mockService.Setup(s => s.CreateSavedSearchAsync("test-user-id", dto))
            .ThrowsAsync(new InvalidOperationException("Create a profile first"));

        var result = await _controller.CreateSavedSearch(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateSavedSearch_Existing_ReturnsOk()
    {
        var dto = new SavedSearchDto { Name = "Updated" };
        _mockService.Setup(s => s.UpdateSavedSearchAsync(1, "test-user-id", dto))
            .ReturnsAsync(new SavedSearch { Id = 1, UserId = "test-user-id", Name = "Updated" });

        var result = await _controller.UpdateSavedSearch(1, dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateSavedSearch_NotFound_ReturnsNotFound()
    {
        var dto = new SavedSearchDto { Name = "X" };
        _mockService.Setup(s => s.UpdateSavedSearchAsync(99, "test-user-id", dto))
            .ReturnsAsync((SavedSearch?)null);

        var result = await _controller.UpdateSavedSearch(99, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteSavedSearch_Existing_ReturnsOk()
    {
        _mockService.Setup(s => s.DeleteSavedSearchAsync(1, "test-user-id")).Returns(Task.CompletedTask);

        var result = await _controller.DeleteSavedSearch(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeleteSavedSearch_NotFound_ReturnsNotFound()
    {
        _mockService.Setup(s => s.DeleteSavedSearchAsync(99, "test-user-id"))
            .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.DeleteSavedSearch(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task RunSavedSearch_Valid_ReturnsOk()
    {
        var matchResult = new { searchName = "Test", matchCount = 1 };
        _mockService.Setup(s => s.RunSavedSearchAsync(1, "test-user-id")).ReturnsAsync((object?)matchResult);

        var result = await _controller.RunSavedSearch(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RunSavedSearch_NotFound_ReturnsNotFound()
    {
        _mockService.Setup(s => s.RunSavedSearchAsync(99, "test-user-id")).ReturnsAsync((object?)null);

        var result = await _controller.RunSavedSearch(99);

        Assert.IsType<NotFoundResult>(result);
    }
}
