using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using retoSquadmakers.Application.DTOs;
using retoSquadmakers.Application.UseCases;
using retoSquadmakers.Application.Interfaces;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Presentation.Controllers;

public static class AuthController
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        // Login local
        app.MapPost("/api/auth/login", LoginAsync)
            .WithName("Login")
            .WithOpenApi()
            .WithSummary("Autenticación local con email y contraseña");

        // Google OAuth login
        app.MapGet("/api/auth/external/google-login", GoogleLoginAsync)
            .WithName("GoogleLogin")
            .WithOpenApi()
            .WithSummary("Iniciar flujo de autenticación con Google");

        // OAuth callback
        app.MapGet("/api/auth/external/callback", CallbackAsync)
            .WithName("ExternalCallback")
            .WithOpenApi()
            .WithSummary("Callback para autenticación OAuth");

        // Crear usuario
        app.MapPost("/api/auth/create-user", CreateUserAsync)
            .WithName("CreateUser")
            .WithOpenApi()
            .WithSummary("Crear usuario")
            .WithDescription("Crea un nuevo usuario. Para crear admin, usar Rol = 'Admin'");

        // Obtener información del usuario autenticado
        app.MapGet("/api/usuario", GetUserInfoAsync)
            .WithName("GetUsuarioInfo")
            .WithOpenApi()
            .WithSummary("Obtener información del usuario autenticado")
            .RequireAuthorization();

        // Endpoint solo para administradores
        app.MapGet("/api/admin", AdminOnlyAsync)
            .WithName("AdminOnly")
            .WithOpenApi()
            .WithSummary("Endpoint exclusivo para administradores")
            .RequireAuthorization("Admin");
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request, 
        LoginUserUseCase loginUseCase)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return Results.BadRequest("Email y contraseña son requeridos");

        var response = await loginUseCase.ExecuteAsync(request);
        
        if (response == null)
            return Results.Unauthorized();

        return Results.Ok(response);
    }

    private static IResult GoogleLoginAsync(string? returnUrl = null)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/api/auth/external/callback"
        };
        
        if (!string.IsNullOrEmpty(returnUrl))
            properties.Items["returnUrl"] = returnUrl;
        
        return Results.Challenge(properties, ["Google"]);
    }

    private static async Task<IResult> CallbackAsync(
        HttpContext context, 
        CreateOrUpdateUserFromOAuthUseCase oauthUseCase)
    {
        try
        {
            // Obtener información del usuario de la autenticación externa
            var result = await context.AuthenticateAsync("Google");
            
            if (!result.Succeeded)
                return Results.BadRequest("Error al autenticar con el proveedor externo");

            var claims = result.Principal.Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
                return Results.BadRequest("No se pudo obtener información del usuario");

            // Ejecutar use case para crear/actualizar usuario
            var response = await oauthUseCase.ExecuteAsync(email, name);

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Error en el callback: {ex.Message}");
        }
    }

    private static async Task<IResult> CreateUserAsync(
        CreateUserRequest request, 
        IUsuarioService usuarioService, 
        IPasswordHasher passwordHasher)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Nombre))
                return Results.BadRequest("Email, password y nombre son requeridos");

            var passwordHash = passwordHasher.HashPassword(request.Password);
            
            var usuario = await usuarioService.CreateUserWithValidationAsync(
                request.Email, 
                request.Nombre, 
                passwordHash, 
                request.Rol);
            
            return Results.Ok(new { 
                mensaje = "Usuario creado exitosamente", 
                usuario = new {
                    id = usuario.Id,
                    email = usuario.Email,
                    nombre = usuario.Nombre,
                    rol = usuario.Rol
                }
            });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return Results.BadRequest(new { error = "Error interno del servidor" });
        }
    }

    private static async Task<IResult> GetUserInfoAsync(
        ClaimsPrincipal user, 
        GetUserInfoUseCase getUserInfoUseCase)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
            return Results.Unauthorized();

        var usuarioInfo = await getUserInfoUseCase.ExecuteAsync(id);
        
        if (usuarioInfo == null)
            return Results.NotFound();

        return Results.Ok(usuarioInfo);
    }

    private static IResult AdminOnlyAsync()
    {
        return Results.Ok(new { 
            mensaje = "Acceso autorizado para administrador", 
            fecha = DateTime.UtcNow 
        });
    }
}