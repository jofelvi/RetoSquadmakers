using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Domain.Repositories;

public interface INotificationRepository : IBaseRepository<Notification, int>
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, int skip = 0, int take = 20);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, int page, int pageSize, string? status = null, string? type = null);
    Task<IEnumerable<Notification>> GetPendingNotificationsAsync(int batchSize = 50);
    Task<IEnumerable<Notification>> GetFailedNotificationsAsync(int maxAttempts = 3);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int notificationId, int userId);
    Task<NotificationStats> GetStatsAsync(int userId);
    Task<bool> UpdateStatusAsync(int notificationId, NotificationStatus status, string? errorMessage = null);
}