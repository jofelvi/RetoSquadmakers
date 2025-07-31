using Microsoft.Extensions.Logging;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Application.EventHandlers;

public class ChisteCreadoEventHandler
{
    private readonly INotificationService _notificationService;
    private readonly IUsuarioRepository _userRepository;
    private readonly ILogger<ChisteCreadoEventHandler> _logger;

    public ChisteCreadoEventHandler(
        INotificationService notificationService,
        IUsuarioRepository userRepository,
        ILogger<ChisteCreadoEventHandler> logger)
    {
        _notificationService = notificationService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task HandleChisteCreatedAsync(Chiste chiste)
    {
        try
        {
            // Get the author of the chiste
            var autor = chiste.Autor ?? await _userRepository.GetByIdAsync(chiste.AutorId);
            if (autor == null)
            {
                _logger.LogWarning("Author not found for chiste {ChisteId}", chiste.Id);
                return;
            }

            _logger.LogInformation("Processing notifications for new chiste {ChisteId} by {AuthorName}", 
                chiste.Id, autor.Nombre);

            // Send email notification to the author
            await SendAuthorNotificationAsync(chiste, autor);

            // Send notifications to admins
            await SendAdminNotificationsAsync(chiste, autor);

            _logger.LogInformation("Notifications processed successfully for chiste {ChisteId}", chiste.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notifications for chiste {ChisteId}: {Error}", 
                chiste.Id, ex.Message);
        }
    }

    private async Task SendAuthorNotificationAsync(Chiste chiste, Usuario autor)
    {
        try
        {
            var notificationRequest = new NotificationRequest
            {
                UserId = autor.Id,
                Type = "Email",
                Subject = "Tu chiste ha sido publicado exitosamente",
                Content = $"¡Hola {autor.Nombre}!\n\nTu chiste ha sido publicado exitosamente en la plataforma.\n\nChiste: \"{chiste.Texto}\"\n\n¡Gracias por compartir tu humor con nosotros!",
                TemplateId = "chiste_created_author",
                TemplateData = new Dictionary<string, object>
                {
                    ["authorName"] = autor.Nombre,
                    ["chisteText"] = chiste.Texto,
                    ["chisteId"] = chiste.Id,
                    ["publishDate"] = chiste.FechaCreacion.ToString("dd/MM/yyyy HH:mm"),
                    ["origen"] = chiste.Origen
                },
                Priority = NotificationPriority.Normal
            };

            var success = await _notificationService.SendNotificationAsync(notificationRequest);
            
            if (success)
            {
                _logger.LogDebug("Author notification sent successfully for chiste {ChisteId}", chiste.Id);
            }
            else
            {
                _logger.LogWarning("Failed to send author notification for chiste {ChisteId}", chiste.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending author notification for chiste {ChisteId}: {Error}", 
                chiste.Id, ex.Message);
        }
    }

    private async Task SendAdminNotificationsAsync(Chiste chiste, Usuario autor)
    {
        try
        {
            // Get all admin users
            var allUsers = await _userRepository.GetAllAsync();
            var adminUsers = allUsers.Where(u => u.Rol == "Admin").ToList();

            if (!adminUsers.Any())
            {
                _logger.LogInformation("No admin users found to notify about chiste {ChisteId}", chiste.Id);
                return;
            }

            var adminNotificationRequests = adminUsers.Select(admin => new NotificationRequest
            {
                UserId = admin.Id,
                Type = "Email",
                Subject = "Nuevo chiste publicado en la plataforma",
                Content = $"Hola {admin.Nombre},\n\nSe ha publicado un nuevo chiste en la plataforma.\n\nAutor: {autor.Nombre}\nChiste: \"{chiste.Texto}\"\nFecha: {chiste.FechaCreacion:dd/MM/yyyy HH:mm}\nOrigen: {chiste.Origen}\n\nPuedes revisar la actividad en el panel de administración.",
                TemplateId = "chiste_created_admin",
                TemplateData = new Dictionary<string, object>
                {
                    ["adminName"] = admin.Nombre,
                    ["authorName"] = autor.Nombre,
                    ["chisteText"] = chiste.Texto,
                    ["chisteId"] = chiste.Id,
                    ["publishDate"] = chiste.FechaCreacion.ToString("dd/MM/yyyy HH:mm"),
                    ["origen"] = chiste.Origen
                },
                Priority = NotificationPriority.Low
            }).ToList();

            var success = await _notificationService.SendBulkNotificationAsync(adminNotificationRequests);
            
            if (success)
            {
                _logger.LogDebug("Admin notifications sent successfully for chiste {ChisteId} to {AdminCount} admins", 
                    chiste.Id, adminUsers.Count);
            }
            else
            {
                _logger.LogWarning("Some admin notifications failed for chiste {ChisteId}", chiste.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending admin notifications for chiste {ChisteId}: {Error}", 
                chiste.Id, ex.Message);
        }
    }
}