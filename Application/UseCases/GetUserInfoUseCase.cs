using retoSquadmakers.Application.DTOs;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Application.UseCases;

public class GetUserInfoUseCase
{
    private readonly IUsuarioService _usuarioService;

    public GetUserInfoUseCase(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    public async Task<UsuarioInfo?> ExecuteAsync(int userId)
    {
        var usuario = await _usuarioService.GetByIdAsync(userId);
        
        if (usuario == null)
            return null;

        return new UsuarioInfo
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email,
            Rol = usuario.Rol,
            FechaCreacion = usuario.FechaCreacion
        };
    }
}