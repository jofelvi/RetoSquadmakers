using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using retoSquadmakers.Application.DTOs;
using retoSquadmakers.Application.UseCases;
using retoSquadmakers.Domain.Services;
using retoSquadmakers.Infrastructure.ExternalServices;
using System.Security.Claims;

namespace retoSquadmakers.Presentation.Controllers;

[ApiController]
[Route("api/chistes")]
[Authorize] // Todos los endpoints requieren autenticación según los requerimientos
public class ChistesController : ControllerBase
{
    private readonly IChisteService _chisteService;
    private readonly IChuckNorrisApiService _chuckNorrisService;
    private readonly IDadJokeApiService _dadJokeService;
    private readonly GetChistesUseCase _getChistesUseCase;
    private readonly CreateChisteUseCase _createChisteUseCase;
    private readonly ILogger<ChistesController> _logger;

    public ChistesController(
        IChisteService chisteService,
        IChuckNorrisApiService chuckNorrisService,
        IDadJokeApiService dadJokeService,
        GetChistesUseCase getChistesUseCase,
        CreateChisteUseCase createChisteUseCase,
        ILogger<ChistesController> logger)
    {
        _chisteService = chisteService;
        _chuckNorrisService = chuckNorrisService;
        _dadJokeService = dadJokeService;
        _getChistesUseCase = getChistesUseCase;
        _createChisteUseCase = createChisteUseCase;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/chistes/aleatorio - Devuelve un chiste aleatorio
    /// </summary>
    [HttpGet("aleatorio")]
    public async Task<IActionResult> GetChisteAleatorio([FromQuery] string? origen = null)
    {
        try
        {
            string chiste;

            switch (origen?.ToLower())
            {
                case "chuck":
                    chiste = await _chuckNorrisService.GetRandomJokeAsync();
                    break;
                case "dad":
                    chiste = await _dadJokeService.GetRandomJokeAsync();
                    break;
                case null:
                    // Si no se especifica origen, elegir aleatoriamente
                    var random = new Random();
                    var choice = random.Next(3); // 0=Chuck, 1=Dad, 2=Local
                    
                    if (choice == 0)
                    {
                        chiste = await _chuckNorrisService.GetRandomJokeAsync();
                        origen = "Chuck";
                    }
                    else if (choice == 1)
                    {
                        chiste = await _dadJokeService.GetRandomJokeAsync();
                        origen = "Dad";
                    }
                    else
                    {
                        var localChistes = await _chisteService.GetRandomLocalChistesAsync(1);
                        var localChiste = localChistes.FirstOrDefault();
                        chiste = localChiste?.Texto ?? "No hay chistes locales disponibles";
                        origen = "Local";
                    }
                    break;
                default:
                    return BadRequest(new { error = "Origen no válido. Use 'Chuck', 'Dad' o deje vacío para aleatorio." });
            }

            return Ok(new { chiste, origen });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener chiste aleatorio");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// GET /api/chistes/emparejados - Empareja chistes de Chuck Norris y Dad Jokes
    /// </summary>
    [HttpGet("emparejados")]
    public async Task<IActionResult> GetChistesEmparejados()
    {
        try
        {
            _logger.LogInformation("Iniciando peticiones paralelas para obtener chistes emparejados");

            // Lanzar 5 peticiones paralelas a cada API usando Task.WhenAll
            var chuckTasks = Enumerable.Range(0, 5)
                .Select(_ => _chuckNorrisService.GetRandomJokeAsync());
            
            var dadTasks = Enumerable.Range(0, 5)
                .Select(_ => _dadJokeService.GetRandomJokeAsync());

            // Ejecutar todas las tareas en paralelo
            var allTasks = chuckTasks.Concat(dadTasks);
            var results = await Task.WhenAll(allTasks);

            // Separar los resultados
            var chuckJokes = results.Take(5).ToList();
            var dadJokes = results.Skip(5).Take(5).ToList();

            // Emparejar los resultados 1 a 1
            var emparejados = chuckJokes.Select((chuck, index) => new
            {
                chuck = chuck,
                dad = dadJokes[index],
                combinado = CombinarChistes(chuck, dadJokes[index])
            }).ToList();

            _logger.LogInformation("Successfully retrieved {Count} paired jokes", emparejados.Count);

            return Ok(emparejados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener chistes emparejados");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// GET /api/chistes/combinado - Combina fragmentos de varios chistes
    /// </summary>
    [HttpGet("combinado")]
    public async Task<IActionResult> GetChisteCombinadoCreativo()
    {
        try
        {
            _logger.LogInformation("Generando chiste combinado creativo");

            // Obtener chistes de múltiples fuentes
            var chuckTask = _chuckNorrisService.GetRandomJokeAsync();
            var dadTask = _dadJokeService.GetRandomJokeAsync();
            var localChistesTask = _chisteService.GetRandomLocalChistesAsync(2);

            await Task.WhenAll(chuckTask, dadTask, localChistesTask);

            var chuck = await chuckTask;
            var dad = await dadTask;
            var localChistes = await localChistesTask;

            // Crear una combinación creativa
            var combinado = CrearChisteCombinadoCreativo(chuck, dad, localChistes.ToList());

            return Ok(new { 
                combinado,
                fuentes = new { chuck, dad, locales = localChistes.Select(c => c.Texto).ToArray() }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar chiste combinado");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// POST /api/chistes - Guarda un nuevo chiste local
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateChiste([FromBody] CreateChisteDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Mapear DTO a la estructura que espera el UseCase
            var request = new CreateChisteRequest 
            { 
                Texto = dto.Texto, 
                Origen = "Local" 
            };
            
            var chiste = await _createChisteUseCase.ExecuteAsync(request, User);
            
            return CreatedAtAction(nameof(GetChiste), new { id = chiste.Id }, chiste);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear chiste");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// GET /api/chistes/filtrar - Filtra chistes locales
    /// </summary>
    [HttpGet("filtrar")]
    public async Task<IActionResult> FiltrarChistes(
        [FromQuery] int? minPalabras = null,
        [FromQuery] string? contiene = null,
        [FromQuery] int? autorId = null,
        [FromQuery] int? tematicaId = null)
    {
        try
        {
            var chistes = await _chisteService.FilterAsync(minPalabras, contiene, autorId, tematicaId);
            
            var result = chistes.Select(c => new
            {
                id = c.Id,
                texto = c.Texto,
                fechaCreacion = c.FechaCreacion,
                autor = c.Autor?.Nombre ?? "Desconocido",
                origen = c.Origen,
                palabras = c.Texto.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al filtrar chistes");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// PUT /api/chistes/{id} - Actualiza un chiste existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateChiste(int id, [FromBody] UpdateChisteDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            var existingChiste = await _chisteService.GetByIdAsync(id);
            if (existingChiste == null)
                return NotFound(new { error = "Chiste no encontrado" });

            // Solo el autor o un admin pueden actualizar
            if (existingChiste.AutorId != userId && userRole != "Admin")
                return Forbid();

            existingChiste.Texto = dto.Texto;
            var updatedChiste = await _chisteService.UpdateAsync(existingChiste);

            return Ok(updatedChiste);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar chiste");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// DELETE /api/chistes/{id} - Elimina un chiste existente
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChiste(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            var existingChiste = await _chisteService.GetByIdAsync(id);
            if (existingChiste == null)
                return NotFound(new { error = "Chiste no encontrado" });

            // Solo el autor o un admin pueden eliminar
            if (existingChiste.AutorId != userId && userRole != "Admin")
                return Forbid();

            await _chisteService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar chiste");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// GET /api/chistes/{id} - Obtiene un chiste por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetChiste(int id)
    {
        try
        {
            var chiste = await _chisteService.GetByIdAsync(id);
            if (chiste == null)
                return NotFound(new { error = "Chiste no encontrado" });

            // Create a DTO to avoid circular reference issues
            var chisteDto = new
            {
                id = chiste.Id,
                texto = chiste.Texto,
                fechaCreacion = chiste.FechaCreacion,
                origen = chiste.Origen,
                autor = chiste.Autor != null ? new
                {
                    id = chiste.Autor.Id,
                    nombre = chiste.Autor.Nombre,
                    email = chiste.Autor.Email
                } : null
            };

            return Ok(chisteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener chiste");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedAccessException("Token de usuario inválido");
        return userId;
    }

    private static string CombinarChistes(string chuck, string dad)
    {
        // Lógica creativa para combinar chistes
        var chuckWords = chuck.Split(' ');
        var dadWords = dad.Split(' ');
        
        // Tomar la primera parte del chiste de Chuck y la segunda del Dad
        var chuckPart = string.Join(" ", chuckWords.Take(chuckWords.Length / 2));
        var dadPart = string.Join(" ", dadWords.Skip(dadWords.Length / 2));
        
        return $"{chuckPart}. Also, {dadPart.ToLower()}";
    }

    private static string CrearChisteCombinadoCreativo(string chuck, string dad, List<Domain.Entities.Chiste> locales)
    {
        var fragments = new List<string>();
        
        // Añadir fragmentos de Chuck Norris
        var chuckWords = chuck.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        if (chuckWords.Length > 0)
            fragments.Add(chuckWords[0].Trim());
            
        // Añadir fragmentos de Dad Jokes
        var dadWords = dad.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        if (dadWords.Length > 1)
            fragments.Add(dadWords[1].Trim());
            
        // Añadir fragmentos de chistes locales
        foreach (var local in locales.Take(2))
        {
            var localWords = local.Texto.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            if (localWords.Length > 0)
                fragments.Add(localWords[0].Trim());
        }
        
        // Conectores creativos
        var conectores = new[] { "Mientras tanto", "Por otro lado", "Además", "Sin embargo", "En consecuencia" };
        var random = new Random();
        
        var resultado = fragments.FirstOrDefault() ?? "Un día cualquiera";
        
        for (int i = 1; i < fragments.Count; i++)
        {
            var conector = conectores[random.Next(conectores.Length)];
            resultado += $". {conector}, {fragments[i].ToLower()}";
        }
        
        return resultado + ".";
    }
}

public class UpdateChisteDto
{
    public string Texto { get; set; } = string.Empty;
}