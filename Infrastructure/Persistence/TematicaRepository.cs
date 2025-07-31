using Microsoft.EntityFrameworkCore;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;

namespace retoSquadmakers.Infrastructure.Persistence;

public class TematicaRepository : BaseRepository<Tematica, int>, ITematicaRepository
{
    public TematicaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Tematica?> GetByNombreAsync(string nombre)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Nombre == nombre);
    }

    public async Task<bool> ExistsByNombreAsync(string nombre)
    {
        return await _dbSet
            .AnyAsync(t => t.Nombre == nombre);
    }
}