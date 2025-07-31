using retoSquadmakers.Domain.Entities;

namespace retoSquadmakers.Domain.Services;

public interface INotificationService
{
    Task<bool> SendNotificationAsync(NotificationRequest request);
    Task<bool> SendBulkNotificationAsync(IEnumerable<NotificationRequest> requests);
    Task<Notification> QueueNotificationAsync(NotificationRequest request);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, int skip = 0, int take = 20);
    Task<bool> MarkAsReadAsync(int notificationId, int userId);
    Task<NotificationStats> GetNotificationStatsAsync(int userId);
}

public class NotificationRequest
{
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty; // Email, SMS, Push
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Recipient { get; set; } // Optional override
    public string? TemplateId { get; set; }
    public Dictionary<string, object>? TemplateData { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
}

public class NotificationStats
{
    public int TotalNotifications { get; set; }
    public int UnreadNotifications { get; set; }
    public int PendingNotifications { get; set; }
    public int FailedNotifications { get; set; }
}