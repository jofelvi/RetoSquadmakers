using retoSquadmakers.Domain.Entities;

namespace retoSquadmakers.Domain.Repositories;

public interface IUsuarioRepository : IBaseRepository<Usuario, int>
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task<IEnumerable<Usuario>> GetByRolAsync(string rol);
}