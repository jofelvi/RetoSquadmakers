using Microsoft.EntityFrameworkCore;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Infrastructure.Persistence;

public class NotificationRepository : BaseRepository<Notification, int>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, int skip = 0, int take = 20)
    {
        return await _dbSet
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Include(n => n.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(
        int userId, int page, int pageSize, string? status = null, string? type = null)
    {
        var query = _dbSet.Where(n => n.UserId == userId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<NotificationStatus>(status, true, out var statusEnum))
        {
            query = query.Where(n => n.Status == statusEnum);
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(n => n.Type == type);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(n => n.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetPendingNotificationsAsync(int batchSize = 50)
    {
        return await _dbSet
            .Where(n => n.Status == NotificationStatus.Pending)
            .OrderBy(n => n.Priority)
            .ThenBy(n => n.CreatedAt)
            .Take(batchSize)
            .Include(n => n.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetFailedNotificationsAsync(int maxAttempts = 3)
    {
        return await _dbSet
            .Where(n => n.Status == NotificationStatus.Failed && n.Attempts < maxAttempts)
            .OrderBy(n => n.Priority)
            .ThenBy(n => n.CreatedAt)
            .Include(n => n.User)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _dbSet
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.Sent)
            .CountAsync();
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _dbSet
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
            return false;

        // In a real implementation, you might have a separate "Read" status or ReadAt timestamp
        // For now, we'll use a different approach or add additional fields to the entity
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<NotificationStats> GetStatsAsync(int userId)
    {
        var notifications = await _dbSet
            .Where(n => n.UserId == userId)
            .GroupBy(n => n.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var totalNotifications = notifications.Sum(n => n.Count);
        var sentCount = notifications.FirstOrDefault(n => n.Status == NotificationStatus.Sent)?.Count ?? 0;
        var pendingCount = notifications.FirstOrDefault(n => n.Status == NotificationStatus.Pending)?.Count ?? 0;
        var failedCount = notifications.FirstOrDefault(n => n.Status == NotificationStatus.Failed)?.Count ?? 0;
        
        var lastNotification = await _dbSet
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .FirstOrDefaultAsync();

        return new NotificationStats
        {
            TotalNotifications = totalNotifications,
            UnreadNotifications = sentCount, // Assuming sent notifications are unread
            PendingNotifications = pendingCount,
            FailedNotifications = failedCount
        };
    }

    public async Task<bool> UpdateStatusAsync(int notificationId, NotificationStatus status, string? errorMessage = null)
    {
        var notification = await _dbSet.FindAsync(notificationId);
        if (notification == null)
            return false;

        notification.Status = status;
        notification.ErrorMessage = errorMessage;
        
        if (status == NotificationStatus.Sent)
        {
            notification.SentAt = DateTime.UtcNow;
        }

        notification.Attempts++;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public override async Task<Notification?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public override async Task<IEnumerable<Notification>> GetAllAsync()
    {
        return await _dbSet
            .Include(n => n.User)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
}