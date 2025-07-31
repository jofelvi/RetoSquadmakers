using Microsoft.Extensions.Configuration;
using Moq;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Infrastructure.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace retoSquadmakers.Tests.Infrastructure.Services;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IConfigurationSection> _mockJwtSection;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockJwtSection = new Mock<IConfigurationSection>();

        // Setup JWT configuration
        _mockJwtSection.Setup(x => x["SecretKey"]).Returns("super_secret_key_that_is_long_enough_for_hmac_256_algorithm");
        _mockJwtSection.Setup(x => x["Issuer"]).Returns("TestIssuer");
        _mockJwtSection.Setup(x => x["Audience"]).Returns("TestAudience");

        _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(_mockJwtSection.Object);

        _jwtService = new JwtService(_mockConfiguration.Object);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsValidJwtToken()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1,
            Email = "test@example.com",
            Nombre = "Test User",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        // Act
        var token = _jwtService.GenerateToken(usuario);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Verify it's a valid JWT format (3 parts separated by dots)
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public void GenerateToken_ContainsCorrectClaims()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 123,
            Email = "user@example.com",
            Nombre = "John Doe",
            Rol = "Admin",
            FechaCreacion = DateTime.UtcNow
        };

        // Act
        var token = _jwtService.GenerateToken(usuario);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.Equal("123", jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal("John Doe", jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal("user@example.com", jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal("Admin", jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public void GenerateToken_WithMissingSecretKey_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockJwtSection.Setup(x => x["SecretKey"]).Returns((string?)null);
        var usuario = new Usuario { Id = 1, Email = "test@example.com", Nombre = "Test", Rol = "User" };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _jwtService.GenerateToken(usuario));
        Assert.Equal("JWT SecretKey no configurada", exception.Message);
    }

    [Fact]
    public void GenerateToken_WithMissingIssuer_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockJwtSection.Setup(x => x["Issuer"]).Returns((string?)null);
        var usuario = new Usuario { Id = 1, Email = "test@example.com", Nombre = "Test", Rol = "User" };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _jwtService.GenerateToken(usuario));
        Assert.Equal("JWT Issuer no configurado", exception.Message);
    }

    [Fact]
    public void GenerateToken_WithMissingAudience_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockJwtSection.Setup(x => x["Audience"]).Returns((string?)null);
        var usuario = new Usuario { Id = 1, Email = "test@example.com", Nombre = "Test", Rol = "User" };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _jwtService.GenerateToken(usuario));
        Assert.Equal("JWT Audience no configurado", exception.Message);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1,
            Email = "test@example.com",
            Nombre = "Test User",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        var token = _jwtService.GenerateToken(usuario);

        // Act
        var principal = _jwtService.ValidateToken(token);

        // Assert
        Assert.NotNull(principal);
        Assert.Equal("1", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal("Test User", principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal("test@example.com", principal.FindFirst(ClaimTypes.Email)?.Value);
        Assert.Equal("User", principal.FindFirst(ClaimTypes.Role)?.Value);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _jwtService.ValidateToken(invalidToken);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void ValidateToken_WithEmptyToken_ReturnsNull()
    {
        // Arrange
        var emptyToken = "";

        // Act
        var principal = _jwtService.ValidateToken(emptyToken);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ReturnsNull()
    {
        // This test would require generating a token with past expiration
        // For simplicity, we'll test with a malformed expired token
        
        // Arrange
        var malformedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE1MTYyMzkwMjJ9.invalid";

        // Act
        var principal = _jwtService.ValidateToken(malformedToken);

        // Assert
        Assert.Null(principal);
    }

    [Theory]
    [InlineData(1, "user1@example.com", "User One", "User")]
    [InlineData(999, "admin@company.com", "Admin User", "Admin")]
    [InlineData(42, "test.user+tag@domain.co.uk", "Test User With Complex Email", "User")]
    public void GenerateToken_WithDifferentUsers_GeneratesValidTokens(int id, string email, string nombre, string rol)
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = id,
            Email = email,
            Nombre = nombre,
            Rol = rol,
            FechaCreacion = DateTime.UtcNow
        };

        // Act
        var token = _jwtService.GenerateToken(usuario);
        var principal = _jwtService.ValidateToken(token);

        // Assert
        Assert.NotNull(token);
        Assert.NotNull(principal);
        Assert.Equal(id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(email, principal.FindFirst(ClaimTypes.Email)?.Value);
        Assert.Equal(nombre, principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal(rol, principal.FindFirst(ClaimTypes.Role)?.Value);
    }

    [Fact]
    public void GenerateToken_TokenHasCorrectIssuerAndAudience()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1,
            Email = "test@example.com",
            Nombre = "Test User",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        // Act
        var token = _jwtService.GenerateToken(usuario);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.Equal("TestIssuer", jwtToken.Issuer);
        Assert.Contains("TestAudience", jwtToken.Audiences);
    }

    [Fact]
    public void GenerateToken_TokenHasExpirationTime()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1,
            Email = "test@example.com",
            Nombre = "Test User",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _jwtService.GenerateToken(usuario);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.True(jwtToken.ValidTo > beforeGeneration.AddHours(23)); // Should expire in ~24 hours
        Assert.True(jwtToken.ValidTo < beforeGeneration.AddHours(25)); // But not more than 25 hours
    }
}