namespace retoSquadmakers.Domain.Entities;

public class NotificationPreference
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty; // Email, SMS, Push
    public string EventType { get; set; } = string.Empty; // ChisteCreated, Welcome, etc.
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Usuario User { get; set; } = null!;
}