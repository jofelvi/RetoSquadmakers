using Microsoft.Extensions.Logging;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;
using retoSquadmakers.Domain.Services;
using System.Text.Json;

namespace retoSquadmakers.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly IUsuarioRepository _userRepository;
    private readonly ITemplateService _templateService;
    private readonly IEnumerable<INotificationProvider> _providers;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository preferenceRepository,
        IUsuarioRepository userRepository,
        ITemplateService templateService,
        IEnumerable<INotificationProvider> providers,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _preferenceRepository = preferenceRepository;
        _userRepository = userRepository;
        _templateService = templateService;
        _providers = providers;
        _logger = logger;
    }

    public async Task<bool> SendNotificationAsync(NotificationRequest request)
    {
        try
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", request.UserId);
                return false;
            }

            // Check user preferences
            var eventType = request.TemplateId ?? "General";
            var isEnabled = await _preferenceRepository.IsEnabledAsync(request.UserId, request.Type, eventType);
            
            if (!isEnabled)
            {
                _logger.LogInformation("Notification disabled by user preference. User: {UserId}, Type: {Type}, Event: {Event}",
                    request.UserId, request.Type, eventType);
                return false;
            }

            // Get the appropriate provider
            var provider = _providers.FirstOrDefault(p => p.CanHandle(request.Type));
            if (provider == null)
            {
                _logger.LogError("No provider found for notification type: {Type}", request.Type);
                return false;
            }

            // Prepare content
            var content = request.Content;
            var subject = request.Subject;

            // Use template if specified
            if (!string.IsNullOrEmpty(request.TemplateId) && request.TemplateData != null)
            {
                try
                {
                    content = await _templateService.RenderTemplateAsync(request.TemplateId, request.TemplateData);
                    
                    // Get template for subject if not provided
                    if (string.IsNullOrEmpty(subject))
                    {
                        var template = await _templateService.GetTemplateAsync(request.TemplateId, request.Type);
                        subject = template?.Subject ?? "";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error rendering template {TemplateId}: {Error}", request.TemplateId, ex.Message);
                    // Fall back to original content
                }
            }

            // Determine recipient
            var recipient = request.Recipient ?? GetDefaultRecipient(user, request.Type);
            if (string.IsNullOrEmpty(recipient))
            {
                _logger.LogWarning("No recipient found for user {UserId} and type {Type}", request.UserId, request.Type);
                return false;
            }

            // Create notification message
            var message = new NotificationMessage
            {
                Recipient = recipient,
                Subject = subject,
                Content = content,
                Metadata = CreateMetadata(request, user)
            };

            // Send notification
            var result = await provider.SendAsync(message);

            // Log the notification
            var notification = new Notification
            {
                UserId = request.UserId,
                Type = request.Type,
                Subject = subject,
                Content = content,
                Recipient = recipient,
                Status = result.IsSuccess ? NotificationStatus.Sent : NotificationStatus.Failed,
                SentAt = result.IsSuccess ? result.SentAt : null,
                ErrorMessage = result.ErrorMessage,
                TemplateId = request.TemplateId,
                TemplateData = request.TemplateData != null ? JsonSerializer.Serialize(request.TemplateData) : null,
                Priority = request.Priority,
                Attempts = 1
            };

            await _notificationRepository.CreateAsync(notification);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Notification sent successfully. User: {UserId}, Type: {Type}, Recipient: {Recipient}",
                    request.UserId, request.Type, recipient);
            }
            else
            {
                _logger.LogWarning("Notification failed. User: {UserId}, Type: {Type}, Error: {Error}",
                    request.UserId, request.Type, result.ErrorMessage);
            }

            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending notification to user {UserId}: {Error}", 
                request.UserId, ex.Message);
            return false;
        }
    }

    public async Task<bool> SendBulkNotificationAsync(IEnumerable<NotificationRequest> requests)
    {
        var tasks = requests.Select(SendNotificationAsync);
        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }

    public async Task<Notification> QueueNotificationAsync(NotificationRequest request)
    {
        // Validate user exists
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {request.UserId} not found");
        }

        // Determine recipient
        var recipient = request.Recipient ?? GetDefaultRecipient(user, request.Type);
        if (string.IsNullOrEmpty(recipient))
        {
            throw new InvalidOperationException($"No recipient found for user {request.UserId} and type {request.Type}");
        }

        // Create pending notification
        var notification = new Notification
        {
            UserId = request.UserId,
            Type = request.Type,
            Subject = request.Subject,
            Content = request.Content,
            Recipient = recipient,
            Status = NotificationStatus.Pending,
            TemplateId = request.TemplateId,
            TemplateData = request.TemplateData != null ? JsonSerializer.Serialize(request.TemplateData) : null,
            Priority = request.Priority
        };

        return await _notificationRepository.CreateAsync(notification);
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, int skip = 0, int take = 20)
    {
        return await _notificationRepository.GetByUserIdAsync(userId, skip, take);
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
    {
        return await _notificationRepository.MarkAsReadAsync(notificationId, userId);
    }

    public async Task<NotificationStats> GetNotificationStatsAsync(int userId)
    {
        return await _notificationRepository.GetStatsAsync(userId);
    }

    private static string GetDefaultRecipient(Usuario user, string notificationType)
    {
        return notificationType.ToLower() switch
        {
            "email" => user.Email,
            "sms" => user.Telefono ?? "", // Assuming Usuario has Telefono property
            "push" => "", // Would need device token from user profile or separate table
            _ => ""
        };
    }

    private static Dictionary<string, object> CreateMetadata(NotificationRequest request, Usuario user)
    {
        return new Dictionary<string, object>
        {
            ["userId"] = request.UserId,
            ["userName"] = user.Nombre,
            ["userEmail"] = user.Email,
            ["priority"] = request.Priority.ToString(),
            ["timestamp"] = DateTime.UtcNow.ToString("O")
        };
    }
}