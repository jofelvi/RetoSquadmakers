using Microsoft.Extensions.Logging;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Infrastructure.Services;

/// <summary>
/// Implementación de SmsNotificador según EJERCICIO 3
/// </summary>
public class SmsNotificador : INotificador
{
    private readonly ILogger<SmsNotificador> _logger;

    public SmsNotificador(ILogger<SmsNotificador> logger)
    {
        _logger = logger;
    }

    public async Task<bool> EnviarAsync(string destinatario, string mensaje)
    {
        try
        {
            // Simulación del envío de SMS según los requerimientos
            _logger.LogInformation("📱 Enviando SMS a: {Destinatario}", destinatario);
            _logger.LogInformation("📱 Mensaje: {Mensaje}", mensaje);
            
            // Simular delay de envío
            await Task.Delay(150);
            
            _logger.LogInformation("✅ SMS enviado exitosamente a {Destinatario}", destinatario);
            
            // Escribir en consola como requiere el ejercicio
            Console.WriteLine($"📱 SMS ENVIADO");
            Console.WriteLine($"   Destinatario: {destinatario}");
            Console.WriteLine($"   Mensaje: {mensaje}");
            Console.WriteLine($"   Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine(new string('-', 50));
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al enviar SMS a {Destinatario}: {Error}", destinatario, ex.Message);
            Console.WriteLine($"❌ ERROR AL ENVIAR SMS: {ex.Message}");
            return false;
        }
    }
}