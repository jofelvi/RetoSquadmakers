using retoSquadmakers.Domain.Entities;

namespace retoSquadmakers.Domain.Repositories;

public interface IChisteRepository : IBaseRepository<Chiste, int>
{
    Task<IEnumerable<Chiste>> GetByAutorIdAsync(int autorId);
    Task<IEnumerable<Chiste>> FilterAsync(int? minPalabras = null, string? contiene = null, int? autorId = null, int? tematicaId = null);
    Task<IEnumerable<Chiste>> GetRandomLocalChistes(int count);
    Task<IEnumerable<Chiste>> GetByOrigenAsync(string origen);
}