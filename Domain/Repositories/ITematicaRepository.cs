using retoSquadmakers.Domain.Entities;

namespace retoSquadmakers.Domain.Repositories;

public interface ITematicaRepository : IBaseRepository<Tematica, int>
{
    Task<Tematica?> GetByNombreAsync(string nombre);
    Task<bool> ExistsByNombreAsync(string nombre);
}