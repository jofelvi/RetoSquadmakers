using System.Security.Claims;
using retoSquadmakers.Application.DTOs;
using retoSquadmakers.Application.UseCases;

namespace retoSquadmakers.Presentation.Controllers;

public static class ChisteController
{
    public static void MapChisteEndpoints(this WebApplication app)
    {
        // Obtener todos los chistes
        app.MapGet("/api/chistes", GetAllChistesAsync)
            .WithName("GetAllChistes")
            .WithOpenApi()
            .WithSummary("Obtener todos los chistes");

        // Obtener chiste por ID
        app.MapGet("/api/chistes/{id:int}", GetChisteByIdAsync)
            .WithName("GetChisteById")
            .WithOpenApi()
            .WithSummary("Obtener chiste por ID");

        // Crear nuevo chiste (requiere autenticación)
        app.MapPost("/api/chistes", CreateChisteAsync)
            .WithName("CreateChiste")
            .WithOpenApi()
            .WithSummary("Crear un nuevo chiste")
            .RequireAuthorization();

        // Filtrar chistes
        app.MapPost("/api/chistes/filter", FilterChistesAsync)
            .WithName("FilterChistes")
            .WithOpenApi()
            .WithSummary("Filtrar chistes por criterios");

        // Obtener chistes aleatorios locales
        app.MapGet("/api/chistes/random/{count:int?}", GetRandomLocalChistesAsync)
            .WithName("GetRandomLocalChistes")
            .WithOpenApi()
            .WithSummary("Obtener chistes aleatorios locales");

        // Obtener chistes por autor (requiere autenticación)
        app.MapGet("/api/chistes/mis-chistes", GetMisChistesAsync)
            .WithName("GetMisChistes")
            .WithOpenApi()
            .WithSummary("Obtener mis chistes")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetAllChistesAsync(GetChistesUseCase getChistesUseCase)
    {
        try
        {
            var chistes = await getChistesUseCase.ExecuteAsync();
            return Results.Ok(chistes);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetChisteByIdAsync(
        int id, 
        GetChistesUseCase getChistesUseCase)
    {
        try
        {
            var chiste = await getChistesUseCase.ExecuteByIdAsync(id);
            
            if (chiste == null)
                return Results.NotFound(new { error = "Chiste no encontrado" });

            return Results.Ok(chiste);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> CreateChisteAsync(
        CreateChisteRequest request,
        ClaimsPrincipal user,
        CreateChisteUseCase createChisteUseCase)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Texto))
                return Results.BadRequest(new { error = "El texto del chiste es requerido" });

            var chiste = await createChisteUseCase.ExecuteAsync(request, user);
            return Results.Created($"/api/chistes/{chiste.Id}", chiste);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Unauthorized();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = "Error interno del servidor" });
        }
    }

    private static async Task<IResult> FilterChistesAsync(
        ChisteFilterRequest filter,
        GetChistesUseCase getChistesUseCase)
    {
        try
        {
            var chistes = await getChistesUseCase.ExecuteFilterAsync(filter);
            return Results.Ok(chistes);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetRandomLocalChistesAsync(
        int? count,
        GetChistesUseCase getChistesUseCase)
    {
        try
        {
            var finalCount = count ?? 5;
            if (finalCount <= 0 || finalCount > 100)
                return Results.BadRequest(new { error = "El número de chistes debe estar entre 1 y 100" });

            var chistes = await getChistesUseCase.ExecuteRandomLocalAsync(finalCount);
            return Results.Ok(chistes);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetMisChistesAsync(
        ClaimsPrincipal user,
        GetChistesUseCase getChistesUseCase)
    {
        try
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int autorId))
            {
                return Results.Unauthorized();
            }

            var filter = new ChisteFilterRequest { AutorId = autorId };
            var chistes = await getChistesUseCase.ExecuteFilterAsync(filter);
            return Results.Ok(chistes);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}