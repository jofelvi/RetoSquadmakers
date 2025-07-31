namespace retoSquadmakers.Application.DTOs;

public class UsuarioInfo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}