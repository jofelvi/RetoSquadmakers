using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using retoSquadmakers.Domain.Services;
using System.Text.Json;

namespace retoSquadmakers.Infrastructure.ExternalServices;

public class PushNotificationProvider : INotificationProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PushNotificationProvider> _logger;

    public string Type => "Push";

    public PushNotificationProvider(IConfiguration configuration, ILogger<PushNotificationProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool CanHandle(string notificationType)
    {
        return string.Equals(notificationType, Type, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<NotificationResult> SendAsync(NotificationMessage message)
    {
        try
        {
            var pushSettings = _configuration.GetSection("PushNotificationSettings");
            var isEnabled = bool.Parse(pushSettings["Enabled"] ?? "false");
            
            if (!isEnabled)
            {
                _logger.LogWarning("Push notification provider is disabled. Skipping push to {Recipient}", message.Recipient);
                return NotificationResult.Failure("Push notification provider is disabled");
            }

            // Validate device token format
            if (!IsValidDeviceToken(message.Recipient))
            {
                _logger.LogWarning("Invalid device token format: {DeviceToken}", message.Recipient);
                return NotificationResult.Failure("Invalid device token format");
            }

            // Create push notification payload
            var payload = CreatePushPayload(message);

            // Simulate network delay
            await Task.Delay(300);

            // Simulate random failures (3% failure rate for testing)
            var random = new Random();
            if (random.NextDouble() < 0.03)
            {
                var errorMessage = "Simulated push notification service failure";
                _logger.LogError("Simulated push notification failure for {DeviceToken}: {Error}", 
                    message.Recipient, errorMessage);
                return NotificationResult.Failure(errorMessage);
            }

            // Log the "sent" push notification
            _logger.LogInformation("Push notification sent successfully to device {DeviceToken} with title: {Title}", 
                message.Recipient, message.Subject);

            // In a real implementation, you would:
            // 1. Use Firebase Cloud Messaging (FCM): await fcmClient.SendAsync(...)
            // 2. Use Apple Push Notification Service (APNs): await apnsClient.SendAsync(...)
            // 3. Use Azure Notification Hubs: await notificationHub.SendAsync(...)

            var externalId = $"push_{DateTime.UtcNow.Ticks}_{random.Next(10000, 99999)}";
            return NotificationResult.Success(externalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending push notification to {DeviceToken}: {Error}", 
                message.Recipient, ex.Message);
            return NotificationResult.Failure($"Error: {ex.Message}");
        }
    }

    private static bool IsValidDeviceToken(string deviceToken)
    {
        if (string.IsNullOrWhiteSpace(deviceToken))
            return false;

        // Basic validation for device token format
        // FCM tokens are typically 152+ characters, APNs tokens are 64 hex characters
        return deviceToken.Length >= 32 && deviceToken.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == ':');
    }

    private static object CreatePushPayload(NotificationMessage message)
    {
        var payload = new
        {
            notification = new
            {
                title = message.Subject,
                body = message.Content,
                icon = "default",
                sound = "default"
            },
            data = message.Metadata ?? new Dictionary<string, object>(),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        return payload;
    }
}