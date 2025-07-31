namespace retoSquadmakers.Domain.Entities;

public class NotificationTemplate
{
    public int Id { get; set; }
    public string TemplateId { get; set; } = string.Empty; // Unique identifier
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Email, SMS, Push
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}