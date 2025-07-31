using Moq;
using retoSquadmakers.Application.Services;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Tests.Application.Services;

public class ChisteServiceTests
{
    private readonly Mock<IChisteRepository> _mockChisteRepository;
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepository;
    private readonly IChisteService _chisteService;

    public ChisteServiceTests()
    {
        _mockChisteRepository = new Mock<IChisteRepository>();
        _mockUsuarioRepository = new Mock<IUsuarioRepository>();
        _chisteService = new ChisteService(_mockChisteRepository.Object, _mockUsuarioRepository.Object);
    }

    [Fact]
    public async Task GetByAutorIdAsync_WithValidAutorId_ReturnsChistes()
    {
        // Arrange
        var autorId = 1;
        var expectedChistes = new List<Chiste>
        {
            new Chiste { Id = 1, Texto = "Chiste 1", AutorId = autorId, Origen = "Local" },
            new Chiste { Id = 2, Texto = "Chiste 2", AutorId = autorId, Origen = "Local" }
        };

        _mockChisteRepository.Setup(r => r.GetByAutorIdAsync(autorId))
                            .ReturnsAsync(expectedChistes);

        // Act
        var result = await _chisteService.GetByAutorIdAsync(autorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Equal(autorId, c.AutorId));
        _mockChisteRepository.Verify(r => r.GetByAutorIdAsync(autorId), Times.Once);
    }

    [Fact]
    public async Task FilterAsync_WithValidParameters_ReturnsFilteredChistes()
    {
        // Arrange
        var minPalabras = 5;
        var contiene = "gracioso";
        var autorId = 1;
        var tematicaId = 2;
        var expectedChistes = new List<Chiste>
        {
            new Chiste { Id = 1, Texto = "Un chiste muy gracioso", AutorId = autorId, Origen = "Local" }
        };

        _mockChisteRepository.Setup(r => r.FilterAsync(minPalabras, contiene, autorId, tematicaId))
                            .ReturnsAsync(expectedChistes);

        // Act
        var result = await _chisteService.FilterAsync(minPalabras, contiene, autorId, tematicaId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("gracioso", result.First().Texto);
        _mockChisteRepository.Verify(r => r.FilterAsync(minPalabras, contiene, autorId, tematicaId), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task GetRandomLocalChistesAsync_WithValidCount_ReturnsChistes(int count)
    {
        // Arrange
        var expectedChistes = Enumerable.Range(1, count)
            .Select(i => new Chiste { Id = i, Texto = $"Chiste {i}", AutorId = 1, Origen = "Local" })
            .ToList();

        _mockChisteRepository.Setup(r => r.GetRandomLocalChistes(count))
                            .ReturnsAsync(expectedChistes);

        // Act
        var result = await _chisteService.GetRandomLocalChistesAsync(count);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(count, result.Count());
        Assert.All(result, c => Assert.Equal("Local", c.Origen));
        _mockChisteRepository.Verify(r => r.GetRandomLocalChistes(count), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task GetRandomLocalChistesAsync_WithInvalidCount_ThrowsArgumentException(int count)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _chisteService.GetRandomLocalChistesAsync(count));

        Assert.Contains("El número de chistes debe ser mayor a 0", exception.Message);
        Assert.Equal("count", exception.ParamName);
        _mockChisteRepository.Verify(r => r.GetRandomLocalChistes(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetRandomLocalChistesAsync_WithCountOver100_ThrowsArgumentException()
    {
        // Arrange
        var count = 101;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _chisteService.GetRandomLocalChistesAsync(count));

        Assert.Contains("No se pueden solicitar más de 100 chistes a la vez", exception.Message);
        Assert.Equal("count", exception.ParamName);
        _mockChisteRepository.Verify(r => r.GetRandomLocalChistes(It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData("Local")]
    [InlineData("API")]
    [InlineData("Importado")]
    public async Task GetByOrigenAsync_WithValidOrigen_ReturnsChistes(string origen)
    {
        // Arrange
        var expectedChistes = new List<Chiste>
        {
            new Chiste { Id = 1, Texto = "Chiste 1", AutorId = 1, Origen = origen },
            new Chiste { Id = 2, Texto = "Chiste 2", AutorId = 1, Origen = origen }
        };

        _mockChisteRepository.Setup(r => r.GetByOrigenAsync(origen))
                            .ReturnsAsync(expectedChistes);

        // Act
        var result = await _chisteService.GetByOrigenAsync(origen);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Equal(origen, c.Origen));
        _mockChisteRepository.Verify(r => r.GetByOrigenAsync(origen), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetByOrigenAsync_WithInvalidOrigen_ThrowsArgumentException(string origen)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _chisteService.GetByOrigenAsync(origen));

        Assert.Contains("El origen no puede ser vacío", exception.Message);
        Assert.Equal("origen", exception.ParamName);
        _mockChisteRepository.Verify(r => r.GetByOrigenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateChisteWithValidationAsync_WithValidData_CreatesChiste()
    {
        // Arrange
        var texto = "Este es un chiste muy gracioso y largo";
        var autorId = 1;
        var origen = "Local";
        var autor = new Usuario { Id = autorId, Email = "test@example.com", Nombre = "Test User" };

        _mockUsuarioRepository.Setup(r => r.GetByIdAsync(autorId))
                             .ReturnsAsync(autor);

        _mockChisteRepository.Setup(r => r.CreateAsync(It.IsAny<Chiste>()))
                            .ReturnsAsync((Chiste c) => { c.Id = 1; return c; });

        // Act
        var result = await _chisteService.CreateChisteWithValidationAsync(texto, autorId, origen);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(texto, result.Texto);
        Assert.Equal(autorId, result.AutorId);
        Assert.Equal(origen, result.Origen);
        Assert.True(result.FechaCreacion <= DateTime.UtcNow);
        
        _mockUsuarioRepository.Verify(r => r.GetByIdAsync(autorId), Times.Exactly(2)); // Once in validation, once in create
        _mockChisteRepository.Verify(r => r.CreateAsync(It.IsAny<Chiste>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateChisteWithValidationAsync_WithInvalidTexto_ThrowsArgumentException(string texto)
    {
        // Arrange
        var autorId = 1;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _chisteService.CreateChisteWithValidationAsync(texto, autorId));

        Assert.Contains("El texto del chiste es requerido", exception.Message);
        Assert.Equal("texto", exception.ParamName);
        _mockUsuarioRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _mockChisteRepository.Verify(r => r.CreateAsync(It.IsAny<Chiste>()), Times.Never);
    }

    [Fact]
    public async Task CreateChisteWithValidationAsync_WithShortText_ThrowsArgumentException()
    {
        // Arrange
        var texto = "Corto";
        var autorId = 1;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _chisteService.CreateChisteWithValidationAsync(texto, autorId));

        Assert.Contains("El chiste debe tener al menos 10 caracteres", exception.Message);
        Assert.Equal("texto", exception.ParamName);
    }

    [Fact]
    public async Task CreateChisteWithValidationAsync_WithLongText_ThrowsArgumentException()
    {
        // Arrange
        var texto = new string('a', 1001);
        var autorId = 1;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _chisteService.CreateChisteWithValidationAsync(texto, autorId));

        Assert.Contains("El chiste no puede exceder 1000 caracteres", exception.Message);
        Assert.Equal("texto", exception.ParamName);
    }

    [Fact]
    public async Task CreateChisteWithValidationAsync_WithNonExistentAutor_ThrowsInvalidOperationException()
    {
        // Arrange
        var texto = "Este es un chiste válido";
        var autorId = 999;

        _mockUsuarioRepository.Setup(r => r.GetByIdAsync(autorId))
                             .ReturnsAsync((Usuario?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _chisteService.CreateChisteWithValidationAsync(texto, autorId));

        Assert.Contains($"No se encontró el usuario con ID {autorId}", exception.Message);
        _mockUsuarioRepository.Verify(r => r.GetByIdAsync(autorId), Times.Once);
        _mockChisteRepository.Verify(r => r.CreateAsync(It.IsAny<Chiste>()), Times.Never);
    }

    [Fact]
    public async Task CreateChisteWithValidationAsync_WithDefaultOrigen_SetsLocalOrigen()
    {
        // Arrange
        var texto = "Este es un chiste con origen por defecto";
        var autorId = 1;
        var autor = new Usuario { Id = autorId, Email = "test@example.com", Nombre = "Test User" };

        _mockUsuarioRepository.Setup(r => r.GetByIdAsync(autorId))
                             .ReturnsAsync(autor);

        _mockChisteRepository.Setup(r => r.CreateAsync(It.IsAny<Chiste>()))
                            .ReturnsAsync((Chiste c) => { c.Id = 1; return c; });

        // Act
        var result = await _chisteService.CreateChisteWithValidationAsync(texto, autorId);

        // Assert
        Assert.Equal("Local", result.Origen);
        _mockChisteRepository.Verify(r => r.CreateAsync(It.Is<Chiste>(c => c.Origen == "Local")), Times.Once);
    }

    [Fact]
    public async Task CreateChisteWithValidationAsync_TrimsTextoAndOrigen()
    {
        // Arrange
        var texto = "  Este es un chiste con espacios  ";
        var origen = "  Custom  ";
        var autorId = 1;
        var autor = new Usuario { Id = autorId, Email = "test@example.com", Nombre = "Test User" };

        _mockUsuarioRepository.Setup(r => r.GetByIdAsync(autorId))
                             .ReturnsAsync(autor);

        _mockChisteRepository.Setup(r => r.CreateAsync(It.IsAny<Chiste>()))
                            .ReturnsAsync((Chiste c) => { c.Id = 1; return c; });

        // Act
        var result = await _chisteService.CreateChisteWithValidationAsync(texto, autorId, origen);

        // Assert
        Assert.Equal("Este es un chiste con espacios", result.Texto);
        Assert.Equal("Custom", result.Origen);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsChiste()
    {
        // Arrange
        var chisteId = 1;
        var expectedChiste = new Chiste
        {
            Id = chisteId,
            Texto = "Chiste de prueba",
            AutorId = 1,
            Origen = "Local"
        };

        _mockChisteRepository.Setup(r => r.GetByIdAsync(chisteId))
                            .ReturnsAsync(expectedChiste);

        // Act
        var result = await _chisteService.GetByIdAsync(chisteId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedChiste.Id, result.Id);
        Assert.Equal(expectedChiste.Texto, result.Texto);
        _mockChisteRepository.Verify(r => r.GetByIdAsync(chisteId), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllChistes()
    {
        // Arrange
        var expectedChistes = new List<Chiste>
        {
            new Chiste { Id = 1, Texto = "Chiste 1", AutorId = 1, Origen = "Local" },
            new Chiste { Id = 2, Texto = "Chiste 2", AutorId = 2, Origen = "API" }
        };

        _mockChisteRepository.Setup(r => r.GetAllAsync())
                            .ReturnsAsync(expectedChistes);

        // Act
        var result = await _chisteService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockChisteRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidChiste_CreatesChiste()
    {
        // Arrange
        var chiste = new Chiste
        {
            Texto = "Este es un chiste válido para crear",
            AutorId = 1,
            Origen = "Local"
        };

        var autor = new Usuario { Id = 1, Email = "test@example.com", Nombre = "Test User" };

        _mockUsuarioRepository.Setup(r => r.GetByIdAsync(chiste.AutorId))
                             .ReturnsAsync(autor);

        _mockChisteRepository.Setup(r => r.CreateAsync(chiste))
                            .ReturnsAsync((Chiste c) => { c.Id = 1; return c; });

        // Act
        var result = await _chisteService.CreateAsync(chiste);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        _mockChisteRepository.Verify(r => r.CreateAsync(chiste), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidChiste_UpdatesChiste()
    {
        // Arrange
        var chiste = new Chiste
        {
            Id = 1,
            Texto = "Chiste actualizado correctamente",
            AutorId = 1,
            Origen = "Local"
        };

        var autor = new Usuario { Id = 1, Email = "test@example.com", Nombre = "Test User" };

        _mockUsuarioRepository.Setup(r => r.GetByIdAsync(chiste.AutorId))
                             .ReturnsAsync(autor);

        _mockChisteRepository.Setup(r => r.UpdateAsync(chiste))
                            .ReturnsAsync(chiste);

        // Act
        var result = await _chisteService.UpdateAsync(chiste);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(chiste.Texto, result.Texto);
        _mockChisteRepository.Verify(r => r.UpdateAsync(chiste), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesChiste()
    {
        // Arrange
        var chisteId = 1;
        var chiste = new Chiste
        {
            Id = chisteId,
            Texto = "Chiste a eliminar",
            AutorId = 1,
            Origen = "Local"
        };

        _mockChisteRepository.Setup(r => r.GetByIdAsync(chisteId))
                            .ReturnsAsync(chiste);

        _mockChisteRepository.Setup(r => r.DeleteAsync(chisteId))
                            .Returns(Task.CompletedTask);

        // Act
        await _chisteService.DeleteAsync(chisteId);

        // Assert
        _mockChisteRepository.Verify(r => r.GetByIdAsync(chisteId), Times.Once);
        _mockChisteRepository.Verify(r => r.DeleteAsync(chisteId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ThrowsInvalidOperationException()
    {
        // Arrange
        var chisteId = 999;

        _mockChisteRepository.Setup(r => r.GetByIdAsync(chisteId))
                            .ReturnsAsync((Chiste?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _chisteService.DeleteAsync(chisteId));

        Assert.Contains($"No se encontró el chiste con ID {chisteId}", exception.Message);
        _mockChisteRepository.Verify(r => r.GetByIdAsync(chisteId), Times.Once);
        _mockChisteRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}