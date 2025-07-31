namespace retoSquadmakers.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string Rol { get; set; } = "User";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public string? Telefono { get; set; } // For SMS notifications
    
    public ICollection<Chiste> Chistes { get; set; } = new List<Chiste>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<NotificationPreference> NotificationPreferences { get; set; } = new List<NotificationPreference>();
}