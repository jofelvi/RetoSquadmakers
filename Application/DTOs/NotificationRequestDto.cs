using retoSquadmakers.Domain.Entities;

namespace retoSquadmakers.Application.DTOs;

public class NotificationRequestDto
{
    public string Type { get; set; } = string.Empty; // Email, SMS, Push
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Recipient { get; set; } // Optional override
    public string? TemplateId { get; set; }
    public Dictionary<string, object>? TemplateData { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
}

public class BulkNotificationRequestDto
{
    public List<int> UserIds { get; set; } = new();
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public Dictionary<string, object>? TemplateData { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
}

public class NotificationResponseDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string Priority { get; set; } = string.Empty;
}

public class NotificationPreferenceDto
{
    public int Id { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

public class SetNotificationPreferenceDto
{
    public string NotificationType { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

public class NotificationTemplateDto
{
    public int Id { get; set; }
    public string TemplateId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateNotificationTemplateDto
{
    public string TemplateId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}