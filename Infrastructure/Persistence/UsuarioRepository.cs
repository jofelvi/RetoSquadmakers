using Microsoft.EntityFrameworkCore;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;

namespace retoSquadmakers.Infrastructure.Persistence;

public class UsuarioRepository : BaseRepository<Usuario, int>, IUsuarioRepository
{
    public UsuarioRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _dbSet
            .AnyAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<Usuario>> GetByRolAsync(string rol)
    {
        return await _dbSet
            .Where(u => u.Rol == rol)
            .ToListAsync();
    }
}