using retoSquadmakers.Domain.Entities;

namespace retoSquadmakers.Domain.Repositories;

public interface INotificationPreferenceRepository : IBaseRepository<NotificationPreference, int>
{
    Task<IEnumerable<NotificationPreference>> GetByUserIdAsync(int userId);
    Task<NotificationPreference?> GetPreferenceAsync(int userId, string notificationType, string eventType);
    Task<bool> IsEnabledAsync(int userId, string notificationType, string eventType);
    Task<NotificationPreference> SetPreferenceAsync(int userId, string notificationType, string eventType, bool isEnabled);
}