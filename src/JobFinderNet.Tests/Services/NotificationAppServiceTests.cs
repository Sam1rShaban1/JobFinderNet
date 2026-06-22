using Moq;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Infrastructure.Services;

namespace JobFinderNet.Tests.Services;

public class NotificationAppServiceTests
{
    private readonly Mock<INotificationRepository> _mockRepo;
    private readonly NotificationAppService _service;

    public NotificationAppServiceTests()
    {
        _mockRepo = new Mock<INotificationRepository>();
        _service = new NotificationAppService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ReturnsMappedDtos()
    {
        var notifications = new List<AppNotification>
        {
            new() { Id = 1, UserId = "u1", Title = "Test", Message = "Msg", IsRead = false, Link = "/link", CreatedAt = DateTime.UtcNow }
        };
        _mockRepo.Setup(r => r.GetUserNotificationsAsync("u1", 20)).ReturnsAsync(notifications);

        var result = await _service.GetUserNotificationsAsync("u1");

        Assert.Single(result);
        Assert.Equal("Test", result[0].Title);
        Assert.Equal("Msg", result[0].Message);
        Assert.False(result[0].IsRead);
        Assert.Equal("/link", result[0].Link);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_EmptyList_ReturnsEmpty()
    {
        _mockRepo.Setup(r => r.GetUserNotificationsAsync("u1", 20)).ReturnsAsync(new List<AppNotification>());

        var result = await _service.GetUserNotificationsAsync("u1");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCount()
    {
        _mockRepo.Setup(r => r.GetUnreadCountAsync("u1")).ReturnsAsync(5);

        var result = await _service.GetUnreadCountAsync("u1");

        Assert.Equal(5, result);
    }

    [Fact]
    public async Task MarkAsReadAsync_ValidNotification_SetsIsRead()
    {
        var notification = new AppNotification { Id = 1, UserId = "u1", Title = "T", Message = "M", IsRead = false };
        _mockRepo.Setup(r => r.GetByIdForUserAsync(1, "u1")).ReturnsAsync(notification);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _service.MarkAsReadAsync(1, "u1");

        Assert.True(notification.IsRead);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_NotFound_ThrowsKeyNotFoundException()
    {
        _mockRepo.Setup(r => r.GetByIdForUserAsync(99, "u1")).ReturnsAsync((AppNotification?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.MarkAsReadAsync(99, "u1"));
    }

    [Fact]
    public async Task MarkAllAsReadAsync_SetsAllUnreadToRead()
    {
        var unread = new List<AppNotification>
        {
            new() { Id = 1, UserId = "u1", Title = "T1", Message = "M1", IsRead = false },
            new() { Id = 2, UserId = "u1", Title = "T2", Message = "M2", IsRead = false }
        };
        _mockRepo.Setup(r => r.GetUnreadForUserAsync("u1")).ReturnsAsync(unread);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _service.MarkAllAsReadAsync("u1");

        Assert.All(unread, n => Assert.True(n.IsRead));
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
