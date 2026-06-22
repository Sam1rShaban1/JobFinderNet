using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Controllers;

namespace JobFinderNet.Tests.Controllers;

public class NotificationsControllerTests
{
    private readonly Mock<INotificationAppService> _mockService;
    private readonly NotificationsController _controller;

    public NotificationsControllerTests()
    {
        _mockService = new Mock<INotificationAppService>();
        _controller = new NotificationsController(_mockService.Object);
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
    public async Task GetNotifications_ReturnsOk()
    {
        _mockService.Setup(s => s.GetUserNotificationsAsync("test-user-id", 20))
            .ReturnsAsync(new List<NotificationDto>
            {
                new() { Id = 1, Title = "Test", Message = "Msg", IsRead = false }
            });

        var result = await _controller.GetNotifications();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var notifications = Assert.IsType<List<NotificationDto>>(okResult.Value);
        Assert.Single(notifications);
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsOk()
    {
        _mockService.Setup(s => s.GetUnreadCountAsync("test-user-id")).ReturnsAsync(3);

        var result = await _controller.GetUnreadCount();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task MarkAsRead_ValidId_ReturnsOk()
    {
        _mockService.Setup(s => s.MarkAsReadAsync(1, "test-user-id")).Returns(Task.CompletedTask);

        var result = await _controller.MarkAsRead(1);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task MarkAsRead_NotFound_ReturnsNotFound()
    {
        _mockService.Setup(s => s.MarkAsReadAsync(99, "test-user-id"))
            .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.MarkAsRead(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task MarkAllAsRead_ReturnsOk()
    {
        _mockService.Setup(s => s.MarkAllAsReadAsync("test-user-id")).Returns(Task.CompletedTask);

        var result = await _controller.MarkAllAsRead();

        Assert.IsType<OkResult>(result);
    }
}
