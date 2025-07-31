using retoSquadmakers.Domain.Entities;

namespace retoSquadmakers.Domain.Services;

public interface ITemplateService
{
    Task<string> RenderTemplateAsync(string templateId, Dictionary<string, object> data);
    Task<NotificationTemplate?> GetTemplateAsync(string templateId, string type);
    Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template);
    Task<NotificationTemplate> UpdateTemplateAsync(NotificationTemplate template);
    Task<bool> DeleteTemplateAsync(string templateId);
    Task<IEnumerable<NotificationTemplate>> GetTemplatesAsync(string? type = null);
}