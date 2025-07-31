using Microsoft.EntityFrameworkCore;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;

namespace retoSquadmakers.Infrastructure.Persistence;

public class NotificationPreferenceRepository : BaseRepository<NotificationPreference, int>, INotificationPreferenceRepository
{
    public NotificationPreferenceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<NotificationPreference>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Where(np => np.UserId == userId)
            .Include(np => np.User)
            .ToListAsync();
    }

    public async Task<NotificationPreference?> GetPreferenceAsync(int userId, string notificationType, string eventType)
    {
        return await _dbSet
            .FirstOrDefaultAsync(np => 
                np.UserId == userId && 
                np.NotificationType == notificationType && 
                np.EventType == eventType);
    }

    public async Task<bool> IsEnabledAsync(int userId, string notificationType, string eventType)
    {
        var preference = await GetPreferenceAsync(userId, notificationType, eventType);
        
        // Default to enabled if no preference is set
        return preference?.IsEnabled ?? true;
    }

    public async Task<NotificationPreference> SetPreferenceAsync(int userId, string notificationType, string eventType, bool isEnabled)
    {
        var existingPreference = await GetPreferenceAsync(userId, notificationType, eventType);

        if (existingPreference != null)
        {
            existingPreference.IsEnabled = isEnabled;
            existingPreference.UpdatedAt = DateTime.UtcNow;
            return await UpdateAsync(existingPreference);
        }
        else
        {
            var newPreference = new NotificationPreference
            {
                UserId = userId,
                NotificationType = notificationType,
                EventType = eventType,
                IsEnabled = isEnabled,
                CreatedAt = DateTime.UtcNow
            };

            return await CreateAsync(newPreference);
        }
    }

    public override async Task<IEnumerable<NotificationPreference>> GetAllAsync()
    {
        return await _dbSet
            .Include(np => np.User)
            .ToListAsync();
    }

    public override async Task<NotificationPreference?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(np => np.User)
            .FirstOrDefaultAsync(np => np.Id == id);
    }
}