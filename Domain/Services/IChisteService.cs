using retoSquadmakers.Domain.Entities;

namespace retoSquadmakers.Domain.Services;

public interface IChisteService : IBaseService<Chiste, int>
{
    Task<IEnumerable<Chiste>> GetByAutorIdAsync(int autorId);
    Task<IEnumerable<Chiste>> FilterAsync(int? minPalabras = null, string? contiene = null, int? autorId = null, int? tematicaId = null);
    Task<IEnumerable<Chiste>> GetRandomLocalChistesAsync(int count);
    Task<IEnumerable<Chiste>> GetByOrigenAsync(string origen);
    Task<Chiste> CreateChisteWithValidationAsync(string texto, int autorId, string origen = "Local");
}