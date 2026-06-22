using JobFinderNet.Core.DTOs;

namespace JobFinderNet.Core.Interfaces.Services;

public interface INotificationAppService
{
    Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int limit = 20);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(int id, string userId);
    Task MarkAllAsReadAsync(string userId);
}
