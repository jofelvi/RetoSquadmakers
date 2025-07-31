using retoSquadmakers.Domain.Entities;

namespace retoSquadmakers.Domain.Services;

public interface IUsuarioService : IBaseService<Usuario, int>
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task<IEnumerable<Usuario>> GetByRolAsync(string rol);
    Task<Usuario> CreateUserWithValidationAsync(string email, string nombre, string? passwordHash, string rol = "User");
}