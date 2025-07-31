using retoSquadmakers.Application.DTOs;
using retoSquadmakers.Domain.Services;
using retoSquadmakers.Application.Interfaces;

namespace retoSquadmakers.Application.UseCases;

public class LoginUserUseCase
{
    private readonly IUsuarioService _usuarioService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserUseCase(
        IUsuarioService usuarioService,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _usuarioService = usuarioService;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse?> ExecuteAsync(LoginRequest request)
    {
        var usuario = await _usuarioService.GetByEmailAsync(request.Email);
        
        if (usuario == null || usuario.PasswordHash == null)
            return null;

        if (!_passwordHasher.VerifyPassword(request.Password, usuario.PasswordHash))
            return null;

        var token = _jwtTokenGenerator.GenerateToken(usuario);

        return new LoginResponse
        {
            Token = token,
            Email = usuario.Email,
            Nombre = usuario.Nombre,
            Rol = usuario.Rol
        };
    }
}