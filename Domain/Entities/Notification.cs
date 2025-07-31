namespace retoSquadmakers.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty; // Email, SMS, Push
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty; // Email address, phone number, etc.
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public int Attempts { get; set; } = 0;
    public string? ErrorMessage { get; set; }
    public string? TemplateId { get; set; }
    public string? TemplateData { get; set; } // JSON data for template
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    // Navigation properties
    public Usuario User { get; set; } = null!;
}

public enum NotificationStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    Cancelled = 3
}

public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}