using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<List<AppNotification>> GetUserNotificationsAsync(string userId, int limit = 20);
    Task<int> GetUnreadCountAsync(string userId);
    Task<AppNotification?> GetByIdAsync(int id);
    Task<AppNotification?> GetByIdForUserAsync(int id, string userId);
    Task<List<AppNotification>> GetUnreadForUserAsync(string userId);
    Task AddAsync(AppNotification notification);
    Task SaveChangesAsync();
}
