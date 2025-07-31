using retoSquadmakers.Application.DTOs;
using retoSquadmakers.Domain.Services;
using retoSquadmakers.Application.Interfaces;

namespace retoSquadmakers.Application.UseCases;

public class CreateOrUpdateUserFromOAuthUseCase
{
    private readonly IUsuarioService _usuarioService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public CreateOrUpdateUserFromOAuthUseCase(
        IUsuarioService usuarioService,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _usuarioService = usuarioService;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> ExecuteAsync(string email, string nombre)
    {
        var existingUser = await _usuarioService.GetByEmailAsync(email);
        
        var usuario = existingUser != null 
            ? await UpdateExistingUser(existingUser, nombre)
            : await _usuarioService.CreateUserWithValidationAsync(email, nombre, null, "User");

        var token = _jwtTokenGenerator.GenerateToken(usuario);

        return new LoginResponse
        {
            Token = token,
            Email = usuario.Email,
            Nombre = usuario.Nombre,
            Rol = usuario.Rol
        };
    }

    private async Task<Domain.Entities.Usuario> UpdateExistingUser(Domain.Entities.Usuario existingUser, string nombre)
    {
        existingUser.Nombre = nombre;
        return await _usuarioService.UpdateAsync(existingUser);
    }
}