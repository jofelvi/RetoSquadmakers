using retoSquadmakers.Domain.Entities;

namespace retoSquadmakers.Domain.Services;

public interface INotificationProvider
{
    string Type { get; }
    Task<NotificationResult> SendAsync(NotificationMessage message);
    bool CanHandle(string notificationType);
}

public class NotificationMessage
{
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class NotificationResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ExternalId { get; set; } // Provider-specific ID
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public static NotificationResult Success(string? externalId = null)
    {
        return new NotificationResult
        {
            IsSuccess = true,
            ExternalId = externalId,
            SentAt = DateTime.UtcNow
        };
    }

    public static NotificationResult Failure(string errorMessage)
    {
        return new NotificationResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            SentAt = DateTime.UtcNow
        };
    }
}