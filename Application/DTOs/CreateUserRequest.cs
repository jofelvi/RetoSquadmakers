namespace retoSquadmakers.Application.DTOs;

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Rol { get; set; } = "User";
}