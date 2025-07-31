using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Infrastructure.ExternalServices;

public class SmsNotificationProvider : INotificationProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsNotificationProvider> _logger;

    public string Type => "SMS";

    public SmsNotificationProvider(IConfiguration configuration, ILogger<SmsNotificationProvider> logger)
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
            // Simulate SMS sending (in a real implementation, this would use Twilio, AWS SNS, etc.)
            var smsSettings = _configuration.GetSection("SmsSettings");
            var isEnabled = bool.Parse(smsSettings["Enabled"] ?? "false");
            var apiKey = smsSettings["ApiKey"];
            var apiSecret = smsSettings["ApiSecret"];
            
            // Check if SMS credentials are configured or if SMS is explicitly disabled
            if (!isEnabled || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                _logger.LogInformation("SMS credentials not configured or disabled. Simulating SMS send to {Recipient}: {Content}", 
                    message.Recipient, TruncateContent(message.Content));
                
                // Simulate network delay
                await Task.Delay(300);
                
                // Return success with simulated ID
                var simulatedId = $"simulated_sms_{DateTime.UtcNow.Ticks}";
                return NotificationResult.Success(simulatedId);
            }

            // Validate phone number format
            if (!IsValidPhoneNumber(message.Recipient))
            {
                _logger.LogWarning("Invalid phone number format: {PhoneNumber}", message.Recipient);
                return NotificationResult.Failure("Invalid phone number format");
            }

            // Simulate network delay
            await Task.Delay(500);

            // Simulate random failures (5% failure rate for testing)
            var random = new Random();
            if (random.NextDouble() < 0.05)
            {
                var errorMessage = "Simulated SMS provider failure";
                _logger.LogError("Simulated SMS failure for {Recipient}: {Error}", message.Recipient, errorMessage);
                return NotificationResult.Failure(errorMessage);
            }

            // Log the "sent" SMS
            _logger.LogInformation("SMS sent successfully to {Recipient}: {Content}", 
                message.Recipient, TruncateContent(message.Content));

            // In a real implementation, you would:
            // 1. Use Twilio SDK: await twilioClient.Messages.CreateAsync(...)
            // 2. Use AWS SNS: await snsClient.PublishAsync(...)
            // 3. Use Azure Communication Services: await smsClient.SendAsync(...)

            var externalId = $"sms_{DateTime.UtcNow.Ticks}_{random.Next(1000, 9999)}";
            return NotificationResult.Success(externalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending SMS to {Recipient}: {Error}", 
                message.Recipient, ex.Message);
            return NotificationResult.Failure($"Error: {ex.Message}");
        }
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Basic phone number validation (international format)
        var cleanNumber = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        
        // Must start with + and have 10-15 digits
        if (!cleanNumber.StartsWith("+"))
            return false;

        var digits = cleanNumber.Substring(1);
        return digits.Length >= 10 && digits.Length <= 15 && digits.All(char.IsDigit);
    }

    private static string TruncateContent(string content, int maxLength = 160)
    {
        if (content.Length <= maxLength)
            return content;

        return content.Substring(0, maxLength - 3) + "...";
    }
}