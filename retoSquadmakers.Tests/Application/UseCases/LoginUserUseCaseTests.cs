using Moq;
using retoSquadmakers.Application.DTOs;
using retoSquadmakers.Application.Interfaces;
using retoSquadmakers.Application.UseCases;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Tests.Application.UseCases;

public class LoginUserUseCaseTests
{
    private readonly Mock<IUsuarioService> _mockUsuarioService;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
    private readonly LoginUserUseCase _loginUserUseCase;

    public LoginUserUseCaseTests()
    {
        _mockUsuarioService = new Mock<IUsuarioService>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        
        _loginUserUseCase = new LoginUserUseCase(
            _mockUsuarioService.Object,
            _mockPasswordHasher.Object,
            _mockJwtTokenGenerator.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var usuario = new Usuario
        {
            Id = 1,
            Email = request.Email,
            Nombre = "Test User",
            PasswordHash = "hashed_password",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        var expectedToken = "jwt_token_123";

        _mockUsuarioService.Setup(s => s.GetByEmailAsync(request.Email))
                          .ReturnsAsync(usuario);
        
        _mockPasswordHasher.Setup(h => h.VerifyPassword(request.Password, usuario.PasswordHash))
                          .Returns(true);
        
        _mockJwtTokenGenerator.Setup(g => g.GenerateToken(usuario))
                             .Returns(expectedToken);

        // Act
        var result = await _loginUserUseCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedToken, result.Token);
        Assert.Equal(usuario.Email, result.Email);
        Assert.Equal(usuario.Nombre, result.Nombre);
        Assert.Equal(usuario.Rol, result.Rol);

        _mockUsuarioService.Verify(s => s.GetByEmailAsync(request.Email), Times.Once);
        _mockPasswordHasher.Verify(h => h.VerifyPassword(request.Password, usuario.PasswordHash), Times.Once);
        _mockJwtTokenGenerator.Verify(g => g.GenerateToken(usuario), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentUser_ReturnsNull()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        _mockUsuarioService.Setup(s => s.GetByEmailAsync(request.Email))
                          .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _loginUserUseCase.ExecuteAsync(request);

        // Assert
        Assert.Null(result);
        _mockUsuarioService.Verify(s => s.GetByEmailAsync(request.Email), Times.Once);
        _mockPasswordHasher.Verify(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockJwtTokenGenerator.Verify(g => g.GenerateToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithUserWithoutPassword_ReturnsNull()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "oauth@example.com",
            Password = "password123"
        };

        var usuario = new Usuario
        {
            Id = 1,
            Email = request.Email,
            Nombre = "OAuth User",
            PasswordHash = null, // OAuth user without local password
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _mockUsuarioService.Setup(s => s.GetByEmailAsync(request.Email))
                          .ReturnsAsync(usuario);

        // Act
        var result = await _loginUserUseCase.ExecuteAsync(request);

        // Assert
        Assert.Null(result);
        _mockUsuarioService.Verify(s => s.GetByEmailAsync(request.Email), Times.Once);
        _mockPasswordHasher.Verify(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockJwtTokenGenerator.Verify(g => g.GenerateToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrong_password"
        };

        var usuario = new Usuario
        {
            Id = 1,
            Email = request.Email,
            Nombre = "Test User",
            PasswordHash = "hashed_password",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _mockUsuarioService.Setup(s => s.GetByEmailAsync(request.Email))
                          .ReturnsAsync(usuario);
        
        _mockPasswordHasher.Setup(h => h.VerifyPassword(request.Password, usuario.PasswordHash))
                          .Returns(false);

        // Act
        var result = await _loginUserUseCase.ExecuteAsync(request);

        // Assert
        Assert.Null(result);
        _mockUsuarioService.Verify(s => s.GetByEmailAsync(request.Email), Times.Once);
        _mockPasswordHasher.Verify(h => h.VerifyPassword(request.Password, usuario.PasswordHash), Times.Once);
        _mockJwtTokenGenerator.Verify(g => g.GenerateToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithAdminUser_ReturnsLoginResponseWithAdminRole()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "admin@example.com",
            Password = "admin_password"
        };

        var usuario = new Usuario
        {
            Id = 1,
            Email = request.Email,
            Nombre = "Admin User",
            PasswordHash = "hashed_admin_password",
            Rol = "Admin",
            FechaCreacion = DateTime.UtcNow
        };

        var expectedToken = "admin_jwt_token";

        _mockUsuarioService.Setup(s => s.GetByEmailAsync(request.Email))
                          .ReturnsAsync(usuario);
        
        _mockPasswordHasher.Setup(h => h.VerifyPassword(request.Password, usuario.PasswordHash))
                          .Returns(true);
        
        _mockJwtTokenGenerator.Setup(g => g.GenerateToken(usuario))
                             .Returns(expectedToken);

        // Act
        var result = await _loginUserUseCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedToken, result.Token);
        Assert.Equal("Admin", result.Rol);
    }
}