using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;
using retoSquadmakers.Domain.Services;
using retoSquadmakers.Application.EventHandlers;
using Microsoft.Extensions.Logging;

namespace retoSquadmakers.Application.Services;

public class ChisteService : BaseService<Chiste, int>, IChisteService
{
    private readonly IChisteRepository _chisteRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ChisteCreadoEventHandler? _eventHandler;
    private readonly ILogger<ChisteService> _logger;

    public ChisteService(
        IChisteRepository chisteRepository, 
        IUsuarioRepository usuarioRepository,
        ILogger<ChisteService> logger,
        ChisteCreadoEventHandler? eventHandler = null) 
        : base(chisteRepository)
    {
        _chisteRepository = chisteRepository;
        _usuarioRepository = usuarioRepository;
        _eventHandler = eventHandler;
        _logger = logger;
    }

    public async Task<IEnumerable<Chiste>> GetByAutorIdAsync(int autorId)
    {
        return await _chisteRepository.GetByAutorIdAsync(autorId);
    }

    public async Task<IEnumerable<Chiste>> FilterAsync(int? minPalabras = null, string? contiene = null, int? autorId = null, int? tematicaId = null)
    {
        return await _chisteRepository.FilterAsync(minPalabras, contiene, autorId, tematicaId);
    }

    public async Task<IEnumerable<Chiste>> GetRandomLocalChistesAsync(int count)
    {
        if (count <= 0)
            throw new ArgumentException("El número de chistes debe ser mayor a 0", nameof(count));

        if (count > 100)
            throw new ArgumentException("No se pueden solicitar más de 100 chistes a la vez", nameof(count));

        return await _chisteRepository.GetRandomLocalChistes(count);
    }

    public async Task<IEnumerable<Chiste>> GetByOrigenAsync(string origen)
    {
        if (string.IsNullOrWhiteSpace(origen))
            throw new ArgumentException("El origen no puede ser vacío", nameof(origen));

        return await _chisteRepository.GetByOrigenAsync(origen);
    }

    public async Task<Chiste> CreateChisteWithValidationAsync(string texto, int autorId, string origen = "Local")
    {
        // Validaciones de dominio
        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("El texto del chiste es requerido", nameof(texto));

        if (texto.Length < 10)
            throw new ArgumentException("El chiste debe tener al menos 10 caracteres", nameof(texto));

        if (texto.Length > 1000)
            throw new ArgumentException("El chiste no puede exceder 1000 caracteres", nameof(texto));

        // Validar que el autor existe
        var autor = await _usuarioRepository.GetByIdAsync(autorId);
        if (autor == null)
            throw new InvalidOperationException($"No se encontró el usuario con ID {autorId}");

        var chiste = new Chiste
        {
            Texto = texto.Trim(),
            AutorId = autorId,
            Origen = string.IsNullOrWhiteSpace(origen) ? "Local" : origen.Trim(),
            FechaCreacion = DateTime.UtcNow
        };

        var createdChiste = await CreateAsync(chiste);

        // Trigger notification event after successful creation
        if (_eventHandler != null)
        {
            try
            {
                // Ensure the author is loaded
                createdChiste.Autor = autor;
                await _eventHandler.HandleChisteCreatedAsync(createdChiste);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chiste created event for chiste {ChisteId}: {Error}", 
                    createdChiste.Id, ex.Message);
                // Don't rethrow - notification failures shouldn't affect chiste creation
            }
        }

        return createdChiste;
    }

    protected override async Task ValidateEntityAsync(Chiste entity, bool isUpdate)
    {
        if (string.IsNullOrWhiteSpace(entity.Texto))
            throw new ArgumentException("El texto del chiste es requerido");

        if (entity.Texto.Length < 10)
            throw new ArgumentException("El chiste debe tener al menos 10 caracteres");

        if (entity.Texto.Length > 1000)
            throw new ArgumentException("El chiste no puede exceder 1000 caracteres");

        if (entity.AutorId <= 0)
            throw new ArgumentException("El ID del autor debe ser válido");

        // Validar que el autor existe
        var autor = await _usuarioRepository.GetByIdAsync(entity.AutorId);
        if (autor == null)
            throw new InvalidOperationException($"No se encontró el usuario con ID {entity.AutorId}");

        await base.ValidateEntityAsync(entity, isUpdate);
    }

    protected override async Task ValidateDeleteAsync(int id)
    {
        var chiste = await GetByIdAsync(id);
        if (chiste == null)
            throw new InvalidOperationException($"No se encontró el chiste con ID {id}");

        // Aquí podrían agregarse más validaciones de dominio
        // Por ejemplo, no permitir eliminar chistes con muchos likes, etc.

        await base.ValidateDeleteAsync(id);
    }
}