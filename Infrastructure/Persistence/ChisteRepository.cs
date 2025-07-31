using Microsoft.EntityFrameworkCore;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;

namespace retoSquadmakers.Infrastructure.Persistence;

public class ChisteRepository : BaseRepository<Chiste, int>, IChisteRepository
{
    public ChisteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Chiste>> GetByAutorIdAsync(int autorId)
    {
        return await _dbSet
            .Where(c => c.AutorId == autorId)
            .Include(c => c.Autor)
            .ToListAsync();
    }

    public async Task<IEnumerable<Chiste>> FilterAsync(int? minPalabras = null, string? contiene = null, int? autorId = null, int? tematicaId = null)
    {
        var query = _dbSet.Include(c => c.Autor).AsQueryable();

        if (minPalabras.HasValue)
        {
            query = query.Where(c => c.Texto.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= minPalabras.Value);
        }

        if (!string.IsNullOrEmpty(contiene))
        {
            query = query.Where(c => c.Texto.Contains(contiene));
        }

        if (autorId.HasValue)
        {
            query = query.Where(c => c.AutorId == autorId.Value);
        }

        if (tematicaId.HasValue)
        {
            query = query.Where(c => c.ChisteTematicas.Any(ct => ct.TematicaId == tematicaId.Value));
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Chiste>> GetRandomLocalChistes(int count)
    {
        return await _dbSet
            .Where(c => c.Origen == "Local")
            .OrderBy(c => Guid.NewGuid())
            .Take(count)
            .Include(c => c.Autor)
            .ToListAsync();
    }

    public async Task<IEnumerable<Chiste>> GetByOrigenAsync(string origen)
    {
        return await _dbSet
            .Where(c => c.Origen == origen)
            .Include(c => c.Autor)
            .ToListAsync();
    }

    public override async Task<IEnumerable<Chiste>> GetAllAsync()
    {
        return await _dbSet
            .Include(c => c.Autor)
            .ToListAsync();
    }

    public override async Task<Chiste?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Autor)
            .Include(c => c.ChisteTematicas)
                .ThenInclude(ct => ct.Tematica)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}