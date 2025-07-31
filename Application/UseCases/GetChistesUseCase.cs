using retoSquadmakers.Application.DTOs;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Application.UseCases;

public class GetChistesUseCase
{
    private readonly IChisteService _chisteService;

    public GetChistesUseCase(IChisteService chisteService)
    {
        _chisteService = chisteService;
    }

    public async Task<IEnumerable<ChisteResponse>> ExecuteAsync()
    {
        var chistes = await _chisteService.GetAllAsync();
        
        return chistes.Select(c => new ChisteResponse
        {
            Id = c.Id,
            Texto = c.Texto,
            FechaCreacion = c.FechaCreacion,
            Origen = c.Origen,
            Autor = new ChisteResponse.AutorInfo
            {
                Id = c.Autor?.Id ?? c.AutorId,
                Nombre = c.Autor?.Nombre ?? "",
                Email = c.Autor?.Email ?? ""
            },
            Tematicas = c.ChisteTematicas?.Select(ct => new ChisteResponse.TematicaInfo
            {
                Id = ct.Tematica?.Id ?? ct.TematicaId,
                Nombre = ct.Tematica?.Nombre ?? ""
            }).ToList() ?? new List<ChisteResponse.TematicaInfo>()
        });
    }

    public async Task<ChisteResponse?> ExecuteByIdAsync(int id)
    {
        var chiste = await _chisteService.GetByIdAsync(id);
        if (chiste == null)
            return null;

        return new ChisteResponse
        {
            Id = chiste.Id,
            Texto = chiste.Texto,
            FechaCreacion = chiste.FechaCreacion,
            Origen = chiste.Origen,
            Autor = new ChisteResponse.AutorInfo
            {
                Id = chiste.Autor?.Id ?? chiste.AutorId,
                Nombre = chiste.Autor?.Nombre ?? "",
                Email = chiste.Autor?.Email ?? ""
            },
            Tematicas = chiste.ChisteTematicas?.Select(ct => new ChisteResponse.TematicaInfo
            {
                Id = ct.Tematica?.Id ?? ct.TematicaId,
                Nombre = ct.Tematica?.Nombre ?? ""
            }).ToList() ?? new List<ChisteResponse.TematicaInfo>()
        };
    }

    public async Task<IEnumerable<ChisteResponse>> ExecuteFilterAsync(ChisteFilterRequest filter)
    {
        var chistes = await _chisteService.FilterAsync(
            filter.MinPalabras, 
            filter.Contiene, 
            filter.AutorId, 
            filter.TematicaId);

        return chistes.Select(c => new ChisteResponse
        {
            Id = c.Id,
            Texto = c.Texto,
            FechaCreacion = c.FechaCreacion,
            Origen = c.Origen,
            Autor = new ChisteResponse.AutorInfo
            {
                Id = c.Autor?.Id ?? c.AutorId,
                Nombre = c.Autor?.Nombre ?? "",
                Email = c.Autor?.Email ?? ""
            },
            Tematicas = c.ChisteTematicas?.Select(ct => new ChisteResponse.TematicaInfo
            {
                Id = ct.Tematica?.Id ?? ct.TematicaId,
                Nombre = ct.Tematica?.Nombre ?? ""
            }).ToList() ?? new List<ChisteResponse.TematicaInfo>()
        });
    }

    public async Task<IEnumerable<ChisteResponse>> ExecuteRandomLocalAsync(int count = 5)
    {
        var chistes = await _chisteService.GetRandomLocalChistesAsync(count);

        return chistes.Select(c => new ChisteResponse
        {
            Id = c.Id,
            Texto = c.Texto,
            FechaCreacion = c.FechaCreacion,
            Origen = c.Origen,
            Autor = new ChisteResponse.AutorInfo
            {
                Id = c.Autor?.Id ?? c.AutorId,
                Nombre = c.Autor?.Nombre ?? "",
                Email = c.Autor?.Email ?? ""
            }
        });
    }
}