using Microsoft.Extensions.Logging;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Infrastructure.Services;

/// <summary>
/// Implementación de EmailNotificador según EJERCICIO 3
/// </summary>
public class EmailNotificador : INotificador
{
    private readonly ILogger<EmailNotificador> _logger;

    public EmailNotificador(ILogger<EmailNotificador> logger)
    {
        _logger = logger;
    }

    public async Task<bool> EnviarAsync(string destinatario, string mensaje)
    {
        try
        {
            // Simulación del envío de email según los requerimientos
            _logger.LogInformation("📧 Enviando EMAIL a: {Destinatario}", destinatario);
            _logger.LogInformation("📧 Mensaje: {Mensaje}", mensaje);
            
            // Simular delay de envío
            await Task.Delay(100);
            
            _logger.LogInformation("✅ EMAIL enviado exitosamente a {Destinatario}", destinatario);
            
            // Escribir en consola como requiere el ejercicio
            Console.WriteLine($"📧 EMAIL ENVIADO");
            Console.WriteLine($"   Destinatario: {destinatario}");
            Console.WriteLine($"   Mensaje: {mensaje}");
            Console.WriteLine($"   Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine(new string('-', 50));
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al enviar EMAIL a {Destinatario}: {Error}", destinatario, ex.Message);
            Console.WriteLine($"❌ ERROR AL ENVIAR EMAIL: {ex.Message}");
            return false;
        }
    }
}