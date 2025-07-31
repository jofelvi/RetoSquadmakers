using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Application.Services;

public class UsuarioService : BaseService<Usuario, int>, IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioService(IUsuarioRepository usuarioRepository) : base(usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _usuarioRepository.GetByEmailAsync(email);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _usuarioRepository.ExistsByEmailAsync(email);
    }

    public async Task<IEnumerable<Usuario>> GetByRolAsync(string rol)
    {
        return await _usuarioRepository.GetByRolAsync(rol);
    }

    public async Task<Usuario> CreateUserWithValidationAsync(string email, string nombre, string? passwordHash, string rol = "User")
    {
        // Validaciones de dominio
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email es requerido", nameof(email));

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es requerido", nameof(nombre));

        if (await ExistsByEmailAsync(email))
            throw new InvalidOperationException($"Ya existe un usuario con el email {email}");

        var usuario = new Usuario
        {
            Email = email,
            Nombre = nombre,
            PasswordHash = passwordHash,
            Rol = rol,
            FechaCreacion = DateTime.UtcNow
        };

        return await CreateAsync(usuario);
    }

    protected override async Task ValidateEntityAsync(Usuario entity, bool isUpdate)
    {
        if (string.IsNullOrWhiteSpace(entity.Email))
            throw new ArgumentException("El email es requerido");

        if (string.IsNullOrWhiteSpace(entity.Nombre))
            throw new ArgumentException("El nombre es requerido");

        if (!isUpdate && await ExistsByEmailAsync(entity.Email))
            throw new InvalidOperationException($"Ya existe un usuario con el email {entity.Email}");

        await base.ValidateEntityAsync(entity, isUpdate);
    }

    protected override async Task ValidateDeleteAsync(int id)
    {
        var usuario = await GetByIdAsync(id);
        if (usuario == null)
            throw new InvalidOperationException($"No se encontró el usuario con ID {id}");

        await base.ValidateDeleteAsync(id);
    }
}