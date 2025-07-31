using Moq;
using retoSquadmakers.Application.DTOs;
using retoSquadmakers.Application.UseCases;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Tests.Application.UseCases;

public class GetUserInfoUseCaseTests
{
    private readonly Mock<IUsuarioService> _mockUsuarioService;
    private readonly GetUserInfoUseCase _getUserInfoUseCase;

    public GetUserInfoUseCaseTests()
    {
        _mockUsuarioService = new Mock<IUsuarioService>();
        _getUserInfoUseCase = new GetUserInfoUseCase(_mockUsuarioService.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingUser_ReturnsUsuarioInfo()
    {
        // Arrange
        var userId = 1;
        var fechaCreacion = DateTime.UtcNow.AddDays(-30);
        var usuario = new Usuario
        {
            Id = userId,
            Email = "test@example.com",
            Nombre = "Test User",
            PasswordHash = "hashed_password",
            Rol = "User",
            FechaCreacion = fechaCreacion
        };

        _mockUsuarioService.Setup(s => s.GetByIdAsync(userId))
                          .ReturnsAsync(usuario);

        // Act
        var result = await _getUserInfoUseCase.ExecuteAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(usuario.Id, result.Id);
        Assert.Equal(usuario.Email, result.Email);
        Assert.Equal(usuario.Nombre, result.Nombre);
        Assert.Equal(usuario.Rol, result.Rol);
        Assert.Equal(usuario.FechaCreacion, result.FechaCreacion);
        
        _mockUsuarioService.Verify(s => s.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentUser_ReturnsNull()
    {
        // Arrange
        var userId = 999;
        _mockUsuarioService.Setup(s => s.GetByIdAsync(userId))
                          .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _getUserInfoUseCase.ExecuteAsync(userId);

        // Assert
        Assert.Null(result);
        _mockUsuarioService.Verify(s => s.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithAdminUser_ReturnsCorrectInfo()
    {
        // Arrange
        var userId = 2;
        var fechaCreacion = DateTime.UtcNow.AddMonths(-2);
        var adminUser = new Usuario
        {
            Id = userId,
            Email = "admin@example.com",
            Nombre = "Admin User",
            PasswordHash = "admin_hashed_password",
            Rol = "Admin",
            FechaCreacion = fechaCreacion
        };

        _mockUsuarioService.Setup(s => s.GetByIdAsync(userId))
                          .ReturnsAsync(adminUser);

        // Act
        var result = await _getUserInfoUseCase.ExecuteAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Admin", result.Rol);
        Assert.Equal("admin@example.com", result.Email);
        Assert.Equal("Admin User", result.Nombre);
    }

    [Fact]
    public async Task ExecuteAsync_WithOAuthUser_ReturnsCorrectInfo()
    {
        // Arrange
        var userId = 3;
        var fechaCreacion = DateTime.UtcNow.AddDays(-5);
        var oauthUser = new Usuario
        {
            Id = userId,
            Email = "oauth@example.com",
            Nombre = "OAuth User",
            PasswordHash = null, // OAuth user without local password
            Rol = "User",
            FechaCreacion = fechaCreacion
        };

        _mockUsuarioService.Setup(s => s.GetByIdAsync(userId))
                          .ReturnsAsync(oauthUser);

        // Act
        var result = await _getUserInfoUseCase.ExecuteAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("oauth@example.com", result.Email);
        Assert.Equal("OAuth User", result.Nombre);
        Assert.Equal("User", result.Rol);
        Assert.Equal(fechaCreacion, result.FechaCreacion);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task ExecuteAsync_WithDifferentUserIds_CallsServiceCorrectly(int userId)
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = userId,
            Email = $"user{userId}@example.com",
            Nombre = $"User {userId}",
            PasswordHash = "password_hash",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _mockUsuarioService.Setup(s => s.GetByIdAsync(userId))
                          .ReturnsAsync(usuario);

        // Act
        var result = await _getUserInfoUseCase.ExecuteAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        _mockUsuarioService.Verify(s => s.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotExposePasswordHash()
    {
        // Arrange
        var userId = 4;
        var usuario = new Usuario
        {
            Id = userId,
            Email = "secure@example.com",
            Nombre = "Secure User",
            PasswordHash = "very_secret_hash",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _mockUsuarioService.Setup(s => s.GetByIdAsync(userId))
                          .ReturnsAsync(usuario);

        // Act
        var result = await _getUserInfoUseCase.ExecuteAsync(userId);

        // Assert
        Assert.NotNull(result);
        
        // Verify UsuarioInfo doesn't have PasswordHash property
        var properties = typeof(UsuarioInfo).GetProperties();
        Assert.DoesNotContain(properties, p => p.Name == "PasswordHash");
        
        // Verify the returned data doesn't contain password information
        Assert.Equal("secure@example.com", result.Email);
        Assert.Equal("Secure User", result.Nombre);
        Assert.Equal("User", result.Rol);
        Assert.Equal(userId, result.Id);
    }
}