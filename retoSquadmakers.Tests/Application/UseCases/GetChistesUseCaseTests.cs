using Moq;
using retoSquadmakers.Application.DTOs;
using retoSquadmakers.Application.UseCases;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Tests.Application.UseCases;

public class GetChistesUseCaseTests
{
    private readonly Mock<IChisteService> _mockChisteService;
    private readonly GetChistesUseCase _getChistesUseCase;

    public GetChistesUseCaseTests()
    {
        _mockChisteService = new Mock<IChisteService>();
        _getChistesUseCase = new GetChistesUseCase(_mockChisteService.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAllChistes()
    {
        // Arrange
        var chistes = new List<Chiste>
        {
            CreateChisteWithAuthor(1, "Chiste 1", 1, "Local"),
            CreateChisteWithAuthor(2, "Chiste 2", 2, "API")
        };

        _mockChisteService.Setup(s => s.GetAllAsync())
                         .ReturnsAsync(chistes);

        // Act
        var result = await _getChistesUseCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        
        var chistesList = result.ToList();
        Assert.Equal("Chiste 1", chistesList[0].Texto);
        Assert.Equal("Chiste 2", chistesList[1].Texto);
        Assert.Equal("User 1", chistesList[0].Autor.Nombre);
        Assert.Equal("User 2", chistesList[1].Autor.Nombre);

        _mockChisteService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyResult_ReturnsEmptyCollection()
    {
        // Arrange
        var emptyChistes = new List<Chiste>();

        _mockChisteService.Setup(s => s.GetAllAsync())
                         .ReturnsAsync(emptyChistes);

        // Act
        var result = await _getChistesUseCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockChisteService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteByIdAsync_WithExistingId_ReturnsChiste()
    {
        // Arrange
        var chisteId = 1;
        var chiste = CreateChisteWithAuthorAndTematicas(chisteId, "Chiste específico", 1, "Local");

        _mockChisteService.Setup(s => s.GetByIdAsync(chisteId))
                         .ReturnsAsync(chiste);

        // Act
        var result = await _getChistesUseCase.ExecuteByIdAsync(chisteId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(chisteId, result.Id);
        Assert.Equal("Chiste específico", result.Texto);
        Assert.Equal("User 1", result.Autor.Nombre);
        Assert.Equal(2, result.Tematicas.Count);
        Assert.Contains(result.Tematicas, t => t.Nombre == "Humor");
        Assert.Contains(result.Tematicas, t => t.Nombre == "Trabajo");

        _mockChisteService.Verify(s => s.GetByIdAsync(chisteId), Times.Once);
    }

    [Fact]
    public async Task ExecuteByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        var chisteId = 999;

        _mockChisteService.Setup(s => s.GetByIdAsync(chisteId))
                         .ReturnsAsync((Chiste?)null);

        // Act
        var result = await _getChistesUseCase.ExecuteByIdAsync(chisteId);

        // Assert
        Assert.Null(result);
        _mockChisteService.Verify(s => s.GetByIdAsync(chisteId), Times.Once);
    }

    [Fact]
    public async Task ExecuteByIdAsync_WithChisteWithoutAuthor_ReturnsResponseWithAuthorId()
    {
        // Arrange
        var chisteId = 1;
        var chiste = new Chiste
        {
            Id = chisteId,
            Texto = "Chiste sin autor",
            AutorId = 5,
            Origen = "API",
            FechaCreacion = DateTime.UtcNow,
            Autor = null, // No author loaded
            ChisteTematicas = new List<ChisteTematica>()
        };

        _mockChisteService.Setup(s => s.GetByIdAsync(chisteId))
                         .ReturnsAsync(chiste);

        // Act
        var result = await _getChistesUseCase.ExecuteByIdAsync(chisteId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Autor.Id);
        Assert.Equal("", result.Autor.Nombre);
        Assert.Equal("", result.Autor.Email);
        Assert.Empty(result.Tematicas);
    }

    [Fact]
    public async Task ExecuteFilterAsync_WithAllFilters_ReturnsFilteredChistes()
    {
        // Arrange
        var filter = new ChisteFilterRequest
        {
            MinPalabras = 5,
            Contiene = "gracioso",
            AutorId = 1,
            TematicaId = 2
        };

        var filteredChistes = new List<Chiste>
        {
            CreateChisteWithAuthor(1, "Un chiste muy gracioso y largo", 1, "Local")
        };

        _mockChisteService.Setup(s => s.FilterAsync(filter.MinPalabras, filter.Contiene, filter.AutorId, filter.TematicaId))
                         .ReturnsAsync(filteredChistes);

        // Act
        var result = await _getChistesUseCase.ExecuteFilterAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var chiste = result.First();
        Assert.Contains("gracioso", chiste.Texto);
        Assert.Equal(1, chiste.Autor.Id);

        _mockChisteService.Verify(s => s.FilterAsync(filter.MinPalabras, filter.Contiene, filter.AutorId, filter.TematicaId), Times.Once);
    }

    [Fact]
    public async Task ExecuteFilterAsync_WithPartialFilters_ReturnsFilteredChistes()
    {
        // Arrange
        var filter = new ChisteFilterRequest
        {
            Contiene = "trabajo",
            AutorId = null,
            TematicaId = null,
            MinPalabras = null
        };

        var filteredChistes = new List<Chiste>
        {
            CreateChisteWithAuthor(1, "Chiste sobre trabajo", 1, "Local"),
            CreateChisteWithAuthor(2, "Otro chiste de trabajo", 2, "API")
        };

        _mockChisteService.Setup(s => s.FilterAsync(null, "trabajo", null, null))
                         .ReturnsAsync(filteredChistes);

        // Act
        var result = await _getChistesUseCase.ExecuteFilterAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Contains("trabajo", c.Texto));

        _mockChisteService.Verify(s => s.FilterAsync(null, "trabajo", null, null), Times.Once);
    }

    [Fact]
    public async Task ExecuteFilterAsync_WithEmptyResult_ReturnsEmptyCollection()
    {
        // Arrange
        var filter = new ChisteFilterRequest
        {
            Contiene = "inexistente"
        };

        var emptyResult = new List<Chiste>();

        _mockChisteService.Setup(s => s.FilterAsync(null, "inexistente", null, null))
                         .ReturnsAsync(emptyResult);

        // Act
        var result = await _getChistesUseCase.ExecuteFilterAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockChisteService.Verify(s => s.FilterAsync(null, "inexistente", null, null), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task ExecuteRandomLocalAsync_WithValidCount_ReturnsRandomChistes(int count)
    {
        // Arrange
        var randomChistes = Enumerable.Range(1, count)
            .Select(i => CreateChisteWithAuthor(i, $"Chiste random {i}", i, "Local"))
            .ToList();

        _mockChisteService.Setup(s => s.GetRandomLocalChistesAsync(count))
                         .ReturnsAsync(randomChistes);

        // Act
        var result = await _getChistesUseCase.ExecuteRandomLocalAsync(count);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(count, result.Count());
        Assert.All(result, c => Assert.Equal("Local", c.Origen));

        _mockChisteService.Verify(s => s.GetRandomLocalChistesAsync(count), Times.Once);
    }

    [Fact]
    public async Task ExecuteRandomLocalAsync_WithDefaultCount_ReturnsFiveChistes()
    {
        // Arrange
        var randomChistes = Enumerable.Range(1, 5)
            .Select(i => CreateChisteWithAuthor(i, $"Chiste random {i}", i, "Local"))
            .ToList();

        _mockChisteService.Setup(s => s.GetRandomLocalChistesAsync(5))
                         .ReturnsAsync(randomChistes);

        // Act
        var result = await _getChistesUseCase.ExecuteRandomLocalAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count());
        _mockChisteService.Verify(s => s.GetRandomLocalChistesAsync(5), Times.Once);
    }

    [Fact]
    public async Task ExecuteRandomLocalAsync_WithEmptyResult_ReturnsEmptyCollection()
    {
        // Arrange
        var emptyResult = new List<Chiste>();

        _mockChisteService.Setup(s => s.GetRandomLocalChistesAsync(5))
                         .ReturnsAsync(emptyResult);

        // Act
        var result = await _getChistesUseCase.ExecuteRandomLocalAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockChisteService.Verify(s => s.GetRandomLocalChistesAsync(5), Times.Once);
    }

    private Chiste CreateChisteWithAuthor(int id, string texto, int autorId, string origen)
    {
        return new Chiste
        {
            Id = id,
            Texto = texto,
            AutorId = autorId,
            Origen = origen,
            FechaCreacion = DateTime.UtcNow,
            Autor = new Usuario
            {
                Id = autorId,
                Email = $"user{autorId}@example.com",
                Nombre = $"User {autorId}"
            },
            ChisteTematicas = new List<ChisteTematica>()
        };
    }

    private Chiste CreateChisteWithAuthorAndTematicas(int id, string texto, int autorId, string origen)
    {
        return new Chiste
        {
            Id = id,
            Texto = texto,
            AutorId = autorId,
            Origen = origen,
            FechaCreacion = DateTime.UtcNow,
            Autor = new Usuario
            {
                Id = autorId,
                Email = $"user{autorId}@example.com",
                Nombre = $"User {autorId}"
            },
            ChisteTematicas = new List<ChisteTematica>
            {
                new ChisteTematica
                {
                    ChisteId = id,
                    TematicaId = 1,
                    Tematica = new Tematica { Id = 1, Nombre = "Humor" }
                },
                new ChisteTematica
                {
                    ChisteId = id,
                    TematicaId = 2,
                    Tematica = new Tematica { Id = 2, Nombre = "Trabajo" }
                }
            }
        };
    }
}