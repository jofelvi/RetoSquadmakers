using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using retoSquadmakers.Application.Services;
using retoSquadmakers.Domain.Services;
using retoSquadmakers.Infrastructure.Services;

namespace retoSquadmakers.Controllers;

[ApiController]
[Route("api/notificaciones")]
[Authorize(Roles = "Admin")] // Solo admins según los requerimientos
public class NotificacionesController : ControllerBase
{
    private readonly ServicioDeAlertas _servicioDeAlertas;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificacionesController> _logger;

    public NotificacionesController(
        ServicioDeAlertas servicioDeAlertas,
        IServiceProvider serviceProvider,
        ILogger<NotificacionesController> logger)
    {
        _servicioDeAlertas = servicioDeAlertas;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/notificaciones/enviar - Envía una notificación según el tipo especificado
    /// </summary>
    [HttpPost("enviar")]
    public async Task<IActionResult> EnviarNotificacion([FromBody] EnviarNotificacionRequest request)
    {
        try
        {
            _logger.LogInformation("Recibida solicitud para enviar notificación tipo: {Tipo}", request.TipoNotificacion);

            if (string.IsNullOrWhiteSpace(request.Destinatario))
                return BadRequest(new { error = "El destinatario es requerido" });

            if (string.IsNullOrWhiteSpace(request.Mensaje))
                return BadRequest(new { error = "El mensaje es requerido" });

            // Determinar el tipo de notificador según el tipo especificado
            INotificador notificador = request.TipoNotificacion?.ToLower() switch
            {
                "email" => _serviceProvider.GetRequiredService<EmailNotificador>(),
                "sms" => _serviceProvider.GetRequiredService<SmsNotificador>(),
                _ => _serviceProvider.GetRequiredService<INotificador>() // Por defecto (EmailNotificador)
            };

            // Crear un nuevo ServicioDeAlertas con el notificador específico
            var servicioEspecifico = new ServicioDeAlertas(notificador, 
                _serviceProvider.GetRequiredService<ILogger<ServicioDeAlertas>>());

            // Enviar la notificación
            var resultado = await servicioEspecifico.EnviarNotificacionAsync(request.Destinatario, request.Mensaje);

            if (resultado)
            {
                _logger.LogInformation("✅ Notificación enviada exitosamente");
                return Ok(new { 
                    mensaje = "Notificación enviada exitosamente",
                    destinatario = request.Destinatario,
                    tipo = request.TipoNotificacion ?? "email",
                    timestamp = DateTime.UtcNow
                });
            }
            else
            {
                _logger.LogWarning("⚠️ La notificación no pudo ser enviada");
                return StatusCode(500, new { error = "No se pudo enviar la notificación" });
            }
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("❌ Parámetros inválidos: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado al enviar notificación");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// POST /api/notificaciones/enviar-alerta - Envía una alerta usando el servicio por defecto
    /// </summary>
    [HttpPost("enviar-alerta")]
    public async Task<IActionResult> EnviarAlerta([FromBody] EnviarAlertaRequest request)
    {
        try
        {
            _logger.LogInformation("Recibida solicitud para enviar alerta");

            if (string.IsNullOrWhiteSpace(request.Destinatario))
                return BadRequest(new { error = "El destinatario es requerido" });

            if (string.IsNullOrWhiteSpace(request.Mensaje))
                return BadRequest(new { error = "El mensaje es requerido" });

            // Usar el ServicioDeAlertas configurado por defecto
            var resultado = await _servicioDeAlertas.EnviarAlertaAsync(request.Destinatario, request.Mensaje);

            if (resultado)
            {
                _logger.LogInformation("✅ Alerta enviada exitosamente");
                return Ok(new { 
                    mensaje = "Alerta enviada exitosamente",
                    destinatario = request.Destinatario,
                    timestamp = DateTime.UtcNow
                });
            }
            else
            {
                _logger.LogWarning("⚠️ La alerta no pudo ser enviada");
                return StatusCode(500, new { error = "No se pudo enviar la alerta" });
            }
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("❌ Parámetros inválidos: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado al enviar alerta");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }
}

/// <summary>
/// Modelo para la solicitud de notificación según EJERCICIO 3
/// </summary>
public class EnviarNotificacionRequest
{
    public string Destinatario { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? TipoNotificacion { get; set; } = "email"; // "email" o "sms"
}

/// <summary>
/// Modelo para la solicitud de alerta
/// </summary>
public class EnviarAlertaRequest
{
    public string Destinatario { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}