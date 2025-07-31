using retoSquadmakers.Domain.Entities;

namespace retoSquadmakers.Domain.Repositories;

public interface INotificationTemplateRepository : IBaseRepository<NotificationTemplate, int>
{
    Task<NotificationTemplate?> GetByTemplateIdAsync(string templateId, string type);
    Task<IEnumerable<NotificationTemplate>> GetByTypeAsync(string type);
    Task<bool> ExistsByTemplateIdAsync(string templateId);
}