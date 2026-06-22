using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Core.Interfaces.Services;

namespace JobFinderNet.Infrastructure.Services;

public class NotificationAppService : INotificationAppService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationAppService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int limit = 20)
    {
        var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, limit);
        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            Link = n.Link,
            CreatedAt = n.CreatedAt
        }).ToList();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(int id, string userId)
    {
        var notification = await _notificationRepository.GetByIdForUserAsync(id, userId);
        if (notification == null)
            throw new KeyNotFoundException("Notification not found");

        notification.IsRead = true;
        await _notificationRepository.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var unread = await _notificationRepository.GetUnreadForUserAsync(userId);
        foreach (var n in unread)
            n.IsRead = true;

        await _notificationRepository.SaveChangesAsync();
    }
}
