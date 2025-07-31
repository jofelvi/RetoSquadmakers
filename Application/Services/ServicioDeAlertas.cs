using Microsoft.Extensions.Logging;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Application.Services;

/// <summary>
/// ServicioDeAlertas según EJERCICIO 3 - Depende de INotificador
/// </summary>
public class ServicioDeAlertas
{
    private readonly INotificador _notificador;
    private readonly ILogger<ServicioDeAlertas> _logger;

    public ServicioDeAlertas(INotificador notificador, ILogger<ServicioDeAlertas> logger)
    {
        _notificador = notificador ?? throw new ArgumentNullException(nameof(notificador));
        _logger = logger;
    }

    /// <summary>
    /// Envía un mensaje de alerta utilizando el notificador configurado
    /// </summary>
    public async Task<bool> EnviarAlertaAsync(string destinatario, string mensaje)
    {
        try
        {
            _logger.LogInformation("🚨 ServicioDeAlertas: Enviando alerta a {Destinatario}", destinatario);
            
            if (string.IsNullOrWhiteSpace(destinatario))
                throw new ArgumentException("El destinatario no puede estar vacío", nameof(destinatario));
            
            if (string.IsNullOrWhiteSpace(mensaje))
                throw new ArgumentException("El mensaje no puede estar vacío", nameof(mensaje));

            // Formatear el mensaje como alerta
            var mensajeAlerta = $"🚨 ALERTA: {mensaje}";
            
            var resultado = await _notificador.EnviarAsync(destinatario, mensajeAlerta);
            
            if (resultado)
            {
                _logger.LogInformation("✅ ServicioDeAlertas: Alerta enviada exitosamente");
            }
            else
            {
                _logger.LogWarning("⚠️ ServicioDeAlertas: La alerta no pudo ser enviada");
            }
            
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ServicioDeAlertas: Error al enviar alerta: {Error}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Envía una notificación regular (sin formato de alerta)
    /// </summary>
    public async Task<bool> EnviarNotificacionAsync(string destinatario, string mensaje)
    {
        try
        {
            _logger.LogInformation("📢 ServicioDeAlertas: Enviando notificación a {Destinatario}", destinatario);
            
            if (string.IsNullOrWhiteSpace(destinatario))
                throw new ArgumentException("El destinatario no puede estar vacío", nameof(destinatario));
            
            if (string.IsNullOrWhiteSpace(mensaje))
                throw new ArgumentException("El mensaje no puede estar vacío", nameof(mensaje));

            var resultado = await _notificador.EnviarAsync(destinatario, mensaje);
            
            if (resultado)
            {
                _logger.LogInformation("✅ ServicioDeAlertas: Notificación enviada exitosamente");
            }
            else
            {
                _logger.LogWarning("⚠️ ServicioDeAlertas: La notificación no pudo ser enviada");
            }
            
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ServicioDeAlertas: Error al enviar notificación: {Error}", ex.Message);
            return false;
        }
    }
}