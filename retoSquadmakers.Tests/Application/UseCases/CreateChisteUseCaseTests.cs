using Moq;
using System.Security.Claims;
using retoSquadmakers.Application.DTOs;
using retoSquadmakers.Application.UseCases;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Tests.Application.UseCases;

public class CreateChisteUseCaseTests
{
    private readonly Mock<IChisteService> _mockChisteService;
    private readonly CreateChisteUseCase _createChisteUseCase;

    public CreateChisteUseCaseTests()
    {
        _mockChisteService = new Mock<IChisteService>();
        _createChisteUseCase = new CreateChisteUseCase(_mockChisteService.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CreatesChiste()
    {
        // Arrange
        var request = new CreateChisteRequest
        {
            Texto = "Este es un chiste muy gracioso",
            Origen = "Local"
        };

        var user = CreateClaimsPrincipal("1", "test@example.com", "Test User");

        var createdChiste = new Chiste
        {
            Id = 1,
            Texto = request.Texto,
            AutorId = 1,
            Origen = request.Origen,
            FechaCreacion = DateTime.UtcNow,
            Autor = new Usuario
            {
                Id = 1,
                Email = "test@example.com",
                Nombre = "Test User"
            }
        };

        _mockChisteService.Setup(s => s.CreateChisteWithValidationAsync(request.Texto, 1, request.Origen))
                         .ReturnsAsync(createdChiste);

        // Act
        var result = await _createChisteUseCase.ExecuteAsync(request, user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdChiste.Id, result.Id);
        Assert.Equal(createdChiste.Texto, result.Texto);
        Assert.Equal(createdChiste.Origen, result.Origen);
        Assert.Equal(createdChiste.Autor.Id, result.Autor.Id);
        Assert.Equal(createdChiste.Autor.Nombre, result.Autor.Nombre);
        Assert.Equal(createdChiste.Autor.Email, result.Autor.Email);

        _mockChisteService.Verify(s => s.CreateChisteWithValidationAsync(request.Texto, 1, request.Origen), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutUserIdClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new CreateChisteRequest
        {
            Texto = "Este es un chiste muy gracioso",
            Origen = "Local"
        };

        var user = new ClaimsPrincipal(); // No claims

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _createChisteUseCase.ExecuteAsync(request, user));

        Assert.Equal("Usuario no autenticado", exception.Message);
        _mockChisteService.Verify(s => s.CreateChisteWithValidationAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidUserIdClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new CreateChisteRequest
        {
            Texto = "Este es un chiste muy gracioso",
            Origen = "Local"
        };

        var user = CreateClaimsPrincipal("invalid", "test@example.com", "Test User");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _createChisteUseCase.ExecuteAsync(request, user));

        Assert.Equal("Usuario no autenticado", exception.Message);
        _mockChisteService.Verify(s => s.CreateChisteWithValidationAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithChisteWithoutAuthor_ReturnsResponseWithAuthorId()
    {
        // Arrange
        var request = new CreateChisteRequest
        {
            Texto = "Este es un chiste sin autor cargado",
            Origen = "API"
        };

        var user = CreateClaimsPrincipal("2", "author@example.com", "Author User");

        var createdChiste = new Chiste
        {
            Id = 2,
            Texto = request.Texto,
            AutorId = 2,
            Origen = request.Origen,
            FechaCreacion = DateTime.UtcNow,
            Autor = null // No author loaded
        };

        _mockChisteService.Setup(s => s.CreateChisteWithValidationAsync(request.Texto, 2, request.Origen))
                         .ReturnsAsync(createdChiste);

        // Act
        var result = await _createChisteUseCase.ExecuteAsync(request, user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdChiste.Id, result.Id);
        Assert.Equal(2, result.Autor.Id);
        Assert.Equal("", result.Autor.Nombre);
        Assert.Equal("", result.Autor.Email);

        _mockChisteService.Verify(s => s.CreateChisteWithValidationAsync(request.Texto, 2, request.Origen), Times.Once);
    }

    [Theory]
    [InlineData("1", "Local")]
    [InlineData("5", "API")]
    [InlineData("10", "Importado")]
    public async Task ExecuteAsync_WithDifferentUsersAndOrigins_CreatesCorrectly(string userId, string origen)
    {
        // Arrange
        var request = new CreateChisteRequest
        {
            Texto = $"Chiste para usuario {userId} con origen {origen}",
            Origen = origen
        };

        var user = CreateClaimsPrincipal(userId, $"user{userId}@example.com", $"User {userId}");
        var userIdInt = int.Parse(userId);

        var createdChiste = new Chiste
        {
            Id = userIdInt,
            Texto = request.Texto,
            AutorId = userIdInt,
            Origen = origen,
            FechaCreacion = DateTime.UtcNow,
            Autor = new Usuario
            {
                Id = userIdInt,
                Email = $"user{userId}@example.com",
                Nombre = $"User {userId}"
            }
        };

        _mockChisteService.Setup(s => s.CreateChisteWithValidationAsync(request.Texto, userIdInt, origen))
                         .ReturnsAsync(createdChiste);

        // Act
        var result = await _createChisteUseCase.ExecuteAsync(request, user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userIdInt, result.Id);
        Assert.Equal(userIdInt, result.Autor.Id);
        Assert.Equal(origen, result.Origen);

        _mockChisteService.Verify(s => s.CreateChisteWithValidationAsync(request.Texto, userIdInt, origen), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsArgumentException_PropagatesException()
    {
        // Arrange
        var request = new CreateChisteRequest
        {
            Texto = "Texto inválido",
            Origen = "Local"
        };

        var user = CreateClaimsPrincipal("1", "test@example.com", "Test User");

        _mockChisteService.Setup(s => s.CreateChisteWithValidationAsync(request.Texto, 1, request.Origen))
                         .ThrowsAsync(new ArgumentException("Texto muy corto"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _createChisteUseCase.ExecuteAsync(request, user));

        Assert.Contains("Texto muy corto", exception.Message);
        _mockChisteService.Verify(s => s.CreateChisteWithValidationAsync(request.Texto, 1, request.Origen), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsInvalidOperationException_PropagatesException()
    {
        // Arrange
        var request = new CreateChisteRequest
        {
            Texto = "Este es un chiste válido",
            Origen = "Local"
        };

        var user = CreateClaimsPrincipal("999", "test@example.com", "Test User");

        _mockChisteService.Setup(s => s.CreateChisteWithValidationAsync(request.Texto, 999, request.Origen))
                         .ThrowsAsync(new InvalidOperationException("Usuario no encontrado"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _createChisteUseCase.ExecuteAsync(request, user));

        Assert.Contains("Usuario no encontrado", exception.Message);
        _mockChisteService.Verify(s => s.CreateChisteWithValidationAsync(request.Texto, 999, request.Origen), Times.Once);
    }

    private ClaimsPrincipal CreateClaimsPrincipal(string userId, string email, string name)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }
}