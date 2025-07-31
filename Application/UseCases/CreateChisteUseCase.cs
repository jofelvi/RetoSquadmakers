using System.Security.Claims;
using retoSquadmakers.Application.DTOs;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Application.UseCases;

public class CreateChisteUseCase
{
    private readonly IChisteService _chisteService;

    public CreateChisteUseCase(IChisteService chisteService)
    {
        _chisteService = chisteService;
    }

    public async Task<ChisteResponse> ExecuteAsync(CreateChisteRequest request, ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int autorId))
        {
            throw new UnauthorizedAccessException("Usuario no autenticado");
        }

        var chiste = await _chisteService.CreateChisteWithValidationAsync(
            request.Texto, 
            autorId, 
            request.Origen);

        return new ChisteResponse
        {
            Id = chiste.Id,
            Texto = chiste.Texto,
            FechaCreacion = chiste.FechaCreacion,
            Origen = chiste.Origen,
            Autor = new ChisteResponse.AutorInfo
            {
                Id = chiste.Autor?.Id ?? autorId,
                Nombre = chiste.Autor?.Nombre ?? "",
                Email = chiste.Autor?.Email ?? ""
            }
        };
    }
}