namespace retoSquadmakers.Domain.Services;

/// <summary>
/// Interfaz para el sistema de notificaciones según EJERCICIO 3
/// </summary>
public interface INotificador
{
    Task<bool> EnviarAsync(string destinatario, string mensaje);
}