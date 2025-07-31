using Moq;
using retoSquadmakers.Application.DTOs;
using retoSquadmakers.Application.Interfaces;
using retoSquadmakers.Application.UseCases;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Tests.Application.UseCases;

public class CreateOrUpdateUserFromOAuthUseCaseTests
{
    private readonly Mock<IUsuarioService> _mockUsuarioService;
    private readonly Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
    private readonly CreateOrUpdateUserFromOAuthUseCase _useCase;

    public CreateOrUpdateUserFromOAuthUseCaseTests()
    {
        _mockUsuarioService = new Mock<IUsuarioService>();
        _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        
        _useCase = new CreateOrUpdateUserFromOAuthUseCase(
            _mockUsuarioService.Object,
            _mockJwtTokenGenerator.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithNewUser_CreatesNewUserAndReturnsToken()
    {
        // Arrange
        var email = "newuser@example.com";
        var nombre = "New OAuth User";
        var expectedToken = "oauth_jwt_token";

        var createdUser = new Usuario
        {
            Id = 1,
            Email = email,
            Nombre = nombre,
            PasswordHash = null,
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _mockUsuarioService.Setup(s => s.GetByEmailAsync(email))
                          .ReturnsAsync((Usuario?)null);
        
        _mockUsuarioService.Setup(s => s.CreateUserWithValidationAsync(email, nombre, null, "User"))
                          .ReturnsAsync(createdUser);
        
        _mockJwtTokenGenerator.Setup(g => g.GenerateToken(createdUser))
                             .Returns(expectedToken);

        // Act
        var result = await _useCase.ExecuteAsync(email, nombre);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedToken, result.Token);
        Assert.Equal(email, result.Email);
        Assert.Equal(nombre, result.Nombre);
        Assert.Equal("User", result.Rol);

        _mockUsuarioService.Verify(s => s.GetByEmailAsync(email), Times.Once);
        _mockUsuarioService.Verify(s => s.CreateUserWithValidationAsync(email, nombre, null, "User"), Times.Once);
        _mockUsuarioService.Verify(s => s.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        _mockJwtTokenGenerator.Verify(g => g.GenerateToken(createdUser), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingUser_UpdatesUserAndReturnsToken()
    {
        // Arrange
        var email = "existing@example.com";
        var nombre = "Updated Name";
        var expectedToken = "updated_jwt_token";

        var existingUser = new Usuario
        {
            Id = 1,
            Email = email,
            Nombre = "Old Name",
            PasswordHash = null,
            Rol = "User",
            FechaCreacion = DateTime.UtcNow.AddDays(-30)
        };

        var updatedUser = new Usuario
        {
            Id = 1,
            Email = email,
            Nombre = nombre,
            PasswordHash = null,
            Rol = "User",
            FechaCreacion = DateTime.UtcNow.AddDays(-30)
        };

        _mockUsuarioService.Setup(s => s.GetByEmailAsync(email))
                          .ReturnsAsync(existingUser);
        
        _mockUsuarioService.Setup(s => s.UpdateAsync(It.IsAny<Usuario>()))
                          .ReturnsAsync(updatedUser);
        
        _mockJwtTokenGenerator.Setup(g => g.GenerateToken(updatedUser))
                             .Returns(expectedToken);

        // Act
        var result = await _useCase.ExecuteAsync(email, nombre);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedToken, result.Token);
        Assert.Equal(email, result.Email);
        Assert.Equal(nombre, result.Nombre);
        Assert.Equal("User", result.Rol);

        _mockUsuarioService.Verify(s => s.GetByEmailAsync(email), Times.Once);
        _mockUsuarioService.Verify(s => s.CreateUserWithValidationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockUsuarioService.Verify(s => s.UpdateAsync(It.Is<Usuario>(u => u.Nombre == nombre)), Times.Once);
        _mockJwtTokenGenerator.Verify(g => g.GenerateToken(updatedUser), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingUserDifferentName_UpdatesNameCorrectly()
    {
        // Arrange
        var email = "user@example.com";
        var oldName = "John Doe";
        var newName = "John Smith";
        var expectedToken = "name_updated_token";

        var existingUser = new Usuario
        {
            Id = 2,
            Email = email,
            Nombre = oldName,
            PasswordHash = "some_hash",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow.AddMonths(-1)
        };

        var updatedUser = new Usuario
        {
            Id = 2,
            Email = email,
            Nombre = newName,
            PasswordHash = "some_hash",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow.AddMonths(-1)
        };

        _mockUsuarioService.Setup(s => s.GetByEmailAsync(email))
                          .ReturnsAsync(existingUser);
        
        _mockUsuarioService.Setup(s => s.UpdateAsync(It.IsAny<Usuario>()))
                          .ReturnsAsync(updatedUser);
        
        _mockJwtTokenGenerator.Setup(g => g.GenerateToken(updatedUser))
                             .Returns(expectedToken);

        // Act
        var result = await _useCase.ExecuteAsync(email, newName);

        // Assert
        Assert.Equal(newName, result.Nombre);
        _mockUsuarioService.Verify(s => s.UpdateAsync(It.Is<Usuario>(u => u.Nombre == newName && u.Email == email)), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingAdminUser_PreservesAdminRole()
    {
        // Arrange
        var email = "admin@example.com";
        var nombre = "Admin User";
        var expectedToken = "admin_oauth_token";

        var existingAdmin = new Usuario
        {
            Id = 3,
            Email = email,
            Nombre = nombre,
            PasswordHash = null,
            Rol = "Admin",
            FechaCreacion = DateTime.UtcNow.AddDays(-10)
        };

        var updatedAdmin = new Usuario
        {
            Id = 3,
            Email = email,
            Nombre = nombre,
            PasswordHash = null,
            Rol = "Admin",
            FechaCreacion = DateTime.UtcNow.AddDays(-10)
        };

        _mockUsuarioService.Setup(s => s.GetByEmailAsync(email))
                          .ReturnsAsync(existingAdmin);
        
        _mockUsuarioService.Setup(s => s.UpdateAsync(It.IsAny<Usuario>()))
                          .ReturnsAsync(updatedAdmin);
        
        _mockJwtTokenGenerator.Setup(g => g.GenerateToken(updatedAdmin))
                             .Returns(expectedToken);

        // Act
        var result = await _useCase.ExecuteAsync(email, nombre);

        // Assert
        Assert.Equal("Admin", result.Rol);
        _mockUsuarioService.Verify(s => s.CreateUserWithValidationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("test@example.com", "Test User")]
    [InlineData("another@domain.org", "Another User")]
    [InlineData("special.chars+test@example.co.uk", "User With Special Email")]
    public async Task ExecuteAsync_WithVariousEmailFormats_HandlesCorrectly(string email, string nombre)
    {
        // Arrange
        var expectedToken = "various_email_token";
        var createdUser = new Usuario
        {
            Id = 99,
            Email = email,
            Nombre = nombre,
            PasswordHash = null,
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _mockUsuarioService.Setup(s => s.GetByEmailAsync(email))
                          .ReturnsAsync((Usuario?)null);
        
        _mockUsuarioService.Setup(s => s.CreateUserWithValidationAsync(email, nombre, null, "User"))
                          .ReturnsAsync(createdUser);
        
        _mockJwtTokenGenerator.Setup(g => g.GenerateToken(createdUser))
                             .Returns(expectedToken);

        // Act
        var result = await _useCase.ExecuteAsync(email, nombre);

        // Assert
        Assert.Equal(email, result.Email);
        Assert.Equal(nombre, result.Nombre);
        Assert.Equal(expectedToken, result.Token);
    }
}