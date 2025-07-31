using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;
using retoSquadmakers.Domain.Services;
using System.Text.Json;

namespace retoSquadmakers.Infrastructure.BackgroundServices;

public class NotificationProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationProcessorService> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(30); // Process every 30 seconds

    public NotificationProcessorService(
        IServiceProvider serviceProvider,
        ILogger<NotificationProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Processor Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingNotificationsAsync();
                await ProcessFailedNotificationsAsync();
                await Task.Delay(_processingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in notification processor service: {Error}", ex.Message);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait longer on error
            }
        }

        _logger.LogInformation("Notification Processor Service stopped");
    }

    private async Task ProcessPendingNotificationsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var providers = scope.ServiceProvider.GetRequiredService<IEnumerable<INotificationProvider>>();
        var templateService = scope.ServiceProvider.GetRequiredService<ITemplateService>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();

        var pendingNotifications = await notificationRepository.GetPendingNotificationsAsync(50);
        
        if (!pendingNotifications.Any())
            return;

        _logger.LogInformation("Processing {Count} pending notifications", pendingNotifications.Count());

        var tasks = pendingNotifications.Select(notification => 
            ProcessSingleNotificationAsync(notification, providers, templateService, userRepository, notificationRepository));

        await Task.WhenAll(tasks);
    }

    private async Task ProcessFailedNotificationsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var providers = scope.ServiceProvider.GetRequiredService<IEnumerable<INotificationProvider>>();
        var templateService = scope.ServiceProvider.GetRequiredService<ITemplateService>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();

        var failedNotifications = await notificationRepository.GetFailedNotificationsAsync(3);
        
        if (!failedNotifications.Any())
            return;

        _logger.LogInformation("Retrying {Count} failed notifications", failedNotifications.Count());

        var tasks = failedNotifications.Select(notification => 
            ProcessSingleNotificationAsync(notification, providers, templateService, userRepository, notificationRepository));

        await Task.WhenAll(tasks);
    }

    private async Task ProcessSingleNotificationAsync(
        Notification notification,
        IEnumerable<INotificationProvider> providers,
        ITemplateService templateService,
        IUsuarioRepository userRepository,
        INotificationRepository notificationRepository)
    {
        try
        {
            // Get the appropriate provider
            var provider = providers.FirstOrDefault(p => p.CanHandle(notification.Type));
            if (provider == null)
            {
                _logger.LogError("No provider found for notification type: {Type}. NotificationId: {Id}", 
                    notification.Type, notification.Id);
                await notificationRepository.UpdateStatusAsync(notification.Id, NotificationStatus.Failed, 
                    $"No provider found for type: {notification.Type}");
                return;
            }

            // Get user for metadata
            var user = notification.User ?? await userRepository.GetByIdAsync(notification.UserId);
            if (user == null)
            {
                _logger.LogError("User not found for notification. UserId: {UserId}, NotificationId: {Id}", 
                    notification.UserId, notification.Id);
                await notificationRepository.UpdateStatusAsync(notification.Id, NotificationStatus.Failed, 
                    "User not found");
                return;
            }

            // Prepare content
            var content = notification.Content;
            var subject = notification.Subject;

            // Use template if specified
            if (!string.IsNullOrEmpty(notification.TemplateId) && !string.IsNullOrEmpty(notification.TemplateData))
            {
                try
                {
                    var templateData = JsonSerializer.Deserialize<Dictionary<string, object>>(notification.TemplateData);
                    if (templateData != null)
                    {
                        content = await templateService.RenderTemplateAsync(notification.TemplateId, templateData);
                        
                        // Get template for subject if not provided
                        if (string.IsNullOrEmpty(subject))
                        {
                            var template = await templateService.GetTemplateAsync(notification.TemplateId, notification.Type);
                            subject = template?.Subject ?? "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error rendering template {TemplateId} for notification {Id}: {Error}", 
                        notification.TemplateId, notification.Id, ex.Message);
                    // Continue with original content
                }
            }

            // Create notification message
            var message = new NotificationMessage
            {
                Recipient = notification.Recipient,
                Subject = subject,
                Content = content,
                Metadata = CreateMetadata(notification, user)
            };

            // Send notification
            var result = await provider.SendAsync(message);

            // Update notification status
            if (result.IsSuccess)
            {
                await notificationRepository.UpdateStatusAsync(notification.Id, NotificationStatus.Sent);
                _logger.LogDebug("Notification sent successfully. Id: {Id}, Type: {Type}, Recipient: {Recipient}",
                    notification.Id, notification.Type, notification.Recipient);
            }
            else
            {
                await notificationRepository.UpdateStatusAsync(notification.Id, NotificationStatus.Failed, result.ErrorMessage);
                _logger.LogWarning("Notification failed. Id: {Id}, Type: {Type}, Error: {Error}",
                    notification.Id, notification.Type, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing notification {Id}: {Error}", 
                notification.Id, ex.Message);
            await notificationRepository.UpdateStatusAsync(notification.Id, NotificationStatus.Failed, ex.Message);
        }
    }

    private static Dictionary<string, object> CreateMetadata(Notification notification, Usuario user)
    {
        return new Dictionary<string, object>
        {
            ["notificationId"] = notification.Id,
            ["userId"] = notification.UserId,
            ["userName"] = user.Nombre,
            ["userEmail"] = user.Email,
            ["priority"] = notification.Priority.ToString(),
            ["createdAt"] = notification.CreatedAt.ToString("O"),
            ["attempts"] = notification.Attempts + 1
        };
    }
}