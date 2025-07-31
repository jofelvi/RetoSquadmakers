using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using retoSquadmakers.Application.DTOs;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;
using retoSquadmakers.Domain.Services;
using System.Security.Claims;

namespace retoSquadmakers.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationService notificationService,
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository preferenceRepository,
        INotificationTemplateRepository templateRepository,
        ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _notificationRepository = notificationRepository;
        _preferenceRepository = preferenceRepository;
        _templateRepository = templateRepository;
        _logger = logger;
    }

    /// <summary>
    /// Send a notification to the current user
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] NotificationRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var notificationRequest = new NotificationRequest
            {
                UserId = userId,
                Type = request.Type,
                Subject = request.Subject,
                Content = request.Content,
                Recipient = request.Recipient,
                TemplateId = request.TemplateId,
                TemplateData = request.TemplateData,
                Priority = request.Priority
            };

            var success = await _notificationService.SendNotificationAsync(notificationRequest);
            
            if (success)
            {
                return Ok(new { message = "Notification sent successfully" });
            }
            else
            {
                return BadRequest(new { error = "Failed to send notification" });
            }
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Send bulk notifications to multiple users
    /// </summary>
    [HttpPost("send-bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendBulkNotification([FromBody] BulkNotificationRequestDto request)
    {
        try
        {
            var notificationRequests = request.UserIds.Select(userId => new NotificationRequest
            {
                UserId = userId,
                Type = request.Type,
                Subject = request.Subject,
                Content = request.Content,
                TemplateId = request.TemplateId,
                TemplateData = request.TemplateData,
                Priority = request.Priority
            });

            var success = await _notificationService.SendBulkNotificationAsync(notificationRequests);
            
            return Ok(new { 
                message = success ? "All notifications sent successfully" : "Some notifications failed",
                success = success,
                userCount = request.UserIds.Count 
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk notification");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get user's notification history
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetNotificationHistory(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationRepository.GetUserNotificationsAsync(
                userId, page, pageSize, status, type);

            var responses = notifications.Select(MapToResponseDto).ToList();
            return Ok(new { notifications = responses, page, pageSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification history");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get notification by ID (user can only access their own notifications)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotification(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var notification = await _notificationRepository.GetByIdAsync(id);

            if (notification == null)
                return NotFound(new { error = "Notification not found" });

            if (notification.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var response = MapToResponseDto(notification);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get user's notification preferences
    /// </summary>
    [HttpGet("preferences")]
    public async Task<IActionResult> GetNotificationPreferences()
    {
        try
        {
            var userId = GetCurrentUserId();
            var preferences = await _preferenceRepository.GetByUserIdAsync(userId);

            var response = preferences.Select(p => new NotificationPreferenceDto
            {
                Id = p.Id,
                NotificationType = p.NotificationType,
                EventType = p.EventType,
                IsEnabled = p.IsEnabled
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification preferences");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update user's notification preference
    /// </summary>
    [HttpPut("preferences")]
    public async Task<IActionResult> SetNotificationPreference([FromBody] SetNotificationPreferenceDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _preferenceRepository.SetPreferenceAsync(
                userId, request.NotificationType, request.EventType, request.IsEnabled);

            return Ok(new { message = "Preference updated successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting notification preference");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get available notification templates
    /// </summary>
    [HttpGet("templates")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetNotificationTemplates([FromQuery] string? type = null)
    {
        try
        {
            var templates = string.IsNullOrEmpty(type) 
                ? await _templateRepository.GetAllAsync()
                : await _templateRepository.GetByTypeAsync(type);

            var response = templates.Select(t => new NotificationTemplateDto
            {
                Id = t.Id,
                TemplateId = t.TemplateId,
                Name = t.Name,
                Type = t.Type,
                Subject = t.Subject,
                Content = t.Content,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification templates");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new notification template
    /// </summary>
    [HttpPost("templates")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateNotificationTemplate([FromBody] CreateNotificationTemplateDto request)
    {
        try
        {
            if (await _templateRepository.ExistsByTemplateIdAsync(request.TemplateId))
                return Conflict(new { error = "Template with this ID already exists" });

            var template = new NotificationTemplate
            {
                TemplateId = request.TemplateId,
                Name = request.Name,
                Type = request.Type,
                Subject = request.Subject,
                Content = request.Content,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _templateRepository.CreateAsync(template);
            
            var response = new NotificationTemplateDto
            {
                Id = template.Id,
                TemplateId = template.TemplateId,
                Name = template.Name,
                Type = template.Type,
                Subject = template.Subject,
                Content = template.Content,
                IsActive = template.IsActive,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            };

            return CreatedAtAction(nameof(GetNotificationTemplate), 
                new { id = template.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification template");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get notification template by ID
    /// </summary>
    [HttpGet("templates/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetNotificationTemplate(int id)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null)
                return NotFound(new { error = "Template not found" });

            var response = new NotificationTemplateDto
            {
                Id = template.Id,
                TemplateId = template.TemplateId,
                Name = template.Name,
                Type = template.Type,
                Subject = template.Subject,
                Content = template.Content,
                IsActive = template.IsActive,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification template");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update notification template
    /// </summary>
    [HttpPut("templates/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateNotificationTemplate(int id, [FromBody] CreateNotificationTemplateDto request)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null)
                return NotFound(new { error = "Template not found" });

            template.Name = request.Name;
            template.Type = request.Type;
            template.Subject = request.Subject;
            template.Content = request.Content;
            template.IsActive = request.IsActive;
            template.UpdatedAt = DateTime.UtcNow;

            await _templateRepository.UpdateAsync(template);

            var response = new NotificationTemplateDto
            {
                Id = template.Id,
                TemplateId = template.TemplateId,
                Name = template.Name,
                Type = template.Type,
                Subject = template.Subject,
                Content = template.Content,
                IsActive = template.IsActive,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification template");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete notification template
    /// </summary>
    [HttpDelete("templates/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteNotificationTemplate(int id)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null)
                return NotFound(new { error = "Template not found" });

            await _templateRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification template");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedAccessException("Invalid user token");
        return userId;
    }

    private static NotificationResponseDto MapToResponseDto(Notification notification)
    {
        return new NotificationResponseDto
        {
            Id = notification.Id,
            Type = notification.Type,
            Subject = notification.Subject,
            Content = notification.Content,
            Recipient = notification.Recipient,
            Status = notification.Status.ToString(),
            CreatedAt = notification.CreatedAt,
            SentAt = notification.SentAt,
            ErrorMessage = notification.ErrorMessage,
            Priority = notification.Priority.ToString()
        };
    }
}