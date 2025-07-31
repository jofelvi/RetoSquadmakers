using Microsoft.EntityFrameworkCore;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;

namespace retoSquadmakers.Infrastructure.Persistence;

public class NotificationTemplateRepository : BaseRepository<NotificationTemplate, int>, INotificationTemplateRepository
{
    public NotificationTemplateRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<NotificationTemplate?> GetByTemplateIdAsync(string templateId, string type)
    {
        return await _dbSet
            .FirstOrDefaultAsync(nt => 
                nt.TemplateId == templateId && 
                nt.Type == type && 
                nt.IsActive);
    }

    public async Task<IEnumerable<NotificationTemplate>> GetByTypeAsync(string type)
    {
        return await _dbSet
            .Where(nt => nt.Type == type)
            .OrderBy(nt => nt.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsByTemplateIdAsync(string templateId)
    {
        return await _dbSet
            .AnyAsync(nt => nt.TemplateId == templateId);
    }

    public override async Task<IEnumerable<NotificationTemplate>> GetAllAsync()
    {
        return await _dbSet
            .OrderBy(nt => nt.Type)
            .ThenBy(nt => nt.Name)
            .ToListAsync();
    }
}