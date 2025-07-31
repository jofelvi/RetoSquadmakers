using Moq;
using retoSquadmakers.Application.Services;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Tests.Domain.Services;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _mockRepository;
    private readonly IUsuarioService _usuarioService;

    public UsuarioServiceTests()
    {
        _mockRepository = new Mock<IUsuarioRepository>();
        _usuarioService = new UsuarioService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ReturnsUsuario()
    {
        // Arrange
        var email = "test@example.com";
        var expectedUsuario = new Usuario
        {
            Id = 1,
            Email = email,
            Nombre = "Test User",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByEmailAsync(email))
                      .ReturnsAsync(expectedUsuario);

        // Act
        var result = await _usuarioService.GetByEmailAsync(email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUsuario.Email, result.Email);
        Assert.Equal(expectedUsuario.Nombre, result.Nombre);
        _mockRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _mockRepository.Setup(r => r.GetByEmailAsync(email))
                      .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _usuarioService.GetByEmailAsync(email);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithExistingEmail_ReturnsTrue()
    {
        // Arrange
        var email = "existing@example.com";
        _mockRepository.Setup(r => r.ExistsByEmailAsync(email))
                      .ReturnsAsync(true);

        // Act
        var result = await _usuarioService.ExistsByEmailAsync(email);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.ExistsByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithNonExistingEmail_ReturnsFalse()
    {
        // Arrange
        var email = "nonexisting@example.com";
        _mockRepository.Setup(r => r.ExistsByEmailAsync(email))
                      .ReturnsAsync(false);

        // Act
        var result = await _usuarioService.ExistsByEmailAsync(email);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.ExistsByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task GetByRolAsync_WithValidRole_ReturnsUsuarios()
    {
        // Arrange
        var rol = "Admin";
        var expectedUsuarios = new List<Usuario>
        {
            new Usuario { Id = 1, Email = "admin1@example.com", Nombre = "Admin 1", Rol = rol },
            new Usuario { Id = 2, Email = "admin2@example.com", Nombre = "Admin 2", Rol = rol }
        };

        _mockRepository.Setup(r => r.GetByRolAsync(rol))
                      .ReturnsAsync(expectedUsuarios);

        // Act
        var result = await _usuarioService.GetByRolAsync(rol);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, u => Assert.Equal(rol, u.Rol));
        _mockRepository.Verify(r => r.GetByRolAsync(rol), Times.Once);
    }

    [Fact]
    public async Task CreateUserWithValidationAsync_WithValidData_CreatesUser()
    {
        // Arrange
        var email = "new@example.com";
        var nombre = "New User";
        var passwordHash = "hashed_password";
        var rol = "User";

        _mockRepository.Setup(r => r.ExistsByEmailAsync(email))
                      .ReturnsAsync(false);
        
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Usuario>()))
                      .ReturnsAsync((Usuario u) => { u.Id = 1; return u; });

        // Act
        var result = await _usuarioService.CreateUserWithValidationAsync(email, nombre, passwordHash, rol);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
        Assert.Equal(nombre, result.Nombre);
        Assert.Equal(passwordHash, result.PasswordHash);
        Assert.Equal(rol, result.Rol);
        _mockRepository.Verify(r => r.ExistsByEmailAsync(email), Times.Exactly(2));
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserWithValidationAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var email = "existing@example.com";
        var nombre = "New User";
        var passwordHash = "hashed_password";

        _mockRepository.Setup(r => r.ExistsByEmailAsync(email))
                      .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _usuarioService.CreateUserWithValidationAsync(email, nombre, passwordHash));

        Assert.Contains($"Ya existe un usuario con el email {email}", exception.Message);
        _mockRepository.Verify(r => r.ExistsByEmailAsync(email), Times.Once);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Theory]
    [InlineData("", "Valid Name", "password")]
    [InlineData("   ", "Valid Name", "password")]
    [InlineData(null, "Valid Name", "password")]
    public async Task CreateUserWithValidationAsync_WithInvalidEmail_ThrowsArgumentException(
        string email, string nombre, string passwordHash)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _usuarioService.CreateUserWithValidationAsync(email, nombre, passwordHash));

        Assert.Contains("El email es requerido", exception.Message);
        Assert.Equal("email", exception.ParamName);
    }

    [Theory]
    [InlineData("valid@example.com", "", "password")]
    [InlineData("valid@example.com", "   ", "password")]
    [InlineData("valid@example.com", null, "password")]
    public async Task CreateUserWithValidationAsync_WithInvalidNombre_ThrowsArgumentException(
        string email, string nombre, string passwordHash)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _usuarioService.CreateUserWithValidationAsync(email, nombre, passwordHash));

        Assert.Contains("El nombre es requerido", exception.Message);
        Assert.Equal("nombre", exception.ParamName);
    }

    [Fact]
    public async Task CreateUserWithValidationAsync_WithDefaultRole_CreatesUserWithUserRole()
    {
        // Arrange
        var email = "new@example.com";
        var nombre = "New User";
        var passwordHash = "hashed_password";

        _mockRepository.Setup(r => r.ExistsByEmailAsync(email))
                      .ReturnsAsync(false);
        
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Usuario>()))
                      .ReturnsAsync((Usuario u) => { u.Id = 1; return u; });

        // Act
        var result = await _usuarioService.CreateUserWithValidationAsync(email, nombre, passwordHash);

        // Assert
        Assert.Equal("User", result.Rol);
        _mockRepository.Verify(r => r.CreateAsync(It.Is<Usuario>(u => u.Rol == "User")), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsUsuario()
    {
        // Arrange
        var userId = 1;
        var expectedUsuario = new Usuario
        {
            Id = userId,
            Email = "test@example.com",
            Nombre = "Test User",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByIdAsync(userId))
                      .ReturnsAsync(expectedUsuario);

        // Act
        var result = await _usuarioService.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUsuario.Id, result.Id);
        Assert.Equal(expectedUsuario.Email, result.Email);
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var userId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
                      .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _usuarioService.GetByIdAsync(userId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsuarios()
    {
        // Arrange
        var expectedUsuarios = new List<Usuario>
        {
            new Usuario { Id = 1, Email = "user1@example.com", Nombre = "User 1", Rol = "User" },
            new Usuario { Id = 2, Email = "user2@example.com", Nombre = "User 2", Rol = "Admin" }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
                      .ReturnsAsync(expectedUsuarios);

        // Act
        var result = await _usuarioService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidUsuario_UpdatesUsuario()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1,
            Email = "updated@example.com",
            Nombre = "Updated User",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.ExistsByEmailAsync(usuario.Email))
                      .ReturnsAsync(false);
        
        _mockRepository.Setup(r => r.UpdateAsync(usuario))
                      .ReturnsAsync(usuario);

        // Act
        var result = await _usuarioService.UpdateAsync(usuario);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(usuario.Email, result.Email);
        _mockRepository.Verify(r => r.UpdateAsync(usuario), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesUsuario()
    {
        // Arrange
        var userId = 1;
        var usuario = new Usuario
        {
            Id = userId,
            Email = "test@example.com",
            Nombre = "Test User",
            Rol = "User"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(userId))
                      .ReturnsAsync(usuario);
        
        _mockRepository.Setup(r => r.DeleteAsync(userId))
                      .Returns(Task.CompletedTask);

        // Act
        await _usuarioService.DeleteAsync(userId);

        // Assert
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(userId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
                      .ReturnsAsync((Usuario?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _usuarioService.DeleteAsync(userId));

        Assert.Contains($"No se encontró el usuario con ID {userId}", exception.Message);
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithValidUsuario_CreatesUsuario()
    {
        // Arrange
        var usuario = new Usuario
        {
            Email = "new@example.com",
            Nombre = "New User",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.ExistsByEmailAsync(usuario.Email))
                      .ReturnsAsync(false);
        
        _mockRepository.Setup(r => r.CreateAsync(usuario))
                      .ReturnsAsync((Usuario u) => { u.Id = 1; return u; });

        // Act
        var result = await _usuarioService.CreateAsync(usuario);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(usuario.Email, result.Email);
        _mockRepository.Verify(r => r.CreateAsync(usuario), Times.Once);
    }

    [Theory]
    [InlineData("", "Valid Name")]
    [InlineData("   ", "Valid Name")]
    [InlineData(null, "Valid Name")]
    public async Task CreateAsync_WithInvalidEmail_ThrowsArgumentException(string email, string nombre)
    {
        // Arrange
        var usuario = new Usuario
        {
            Email = email,
            Nombre = nombre,
            Rol = "User"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _usuarioService.CreateAsync(usuario));

        Assert.Contains("El email es requerido", exception.Message);
    }

    [Theory]
    [InlineData("valid@example.com", "")]
    [InlineData("valid@example.com", "   ")]
    [InlineData("valid@example.com", null)]
    public async Task CreateAsync_WithInvalidNombre_ThrowsArgumentException(string email, string nombre)
    {
        // Arrange
        var usuario = new Usuario
        {
            Email = email,
            Nombre = nombre,
            Rol = "User"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _usuarioService.CreateAsync(usuario));

        Assert.Contains("El nombre es requerido", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var usuario = new Usuario
        {
            Email = "duplicate@example.com",
            Nombre = "User",
            Rol = "User"
        };

        _mockRepository.Setup(r => r.ExistsByEmailAsync(usuario.Email))
                      .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _usuarioService.CreateAsync(usuario));

        Assert.Contains($"Ya existe un usuario con el email {usuario.Email}", exception.Message);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidEmail_ThrowsArgumentException()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1,
            Email = "",
            Nombre = "Valid Name",
            Rol = "User"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _usuarioService.UpdateAsync(usuario));

        Assert.Contains("El email es requerido", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidNombre_ThrowsArgumentException()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1,
            Email = "valid@example.com",
            Nombre = "",
            Rol = "User"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _usuarioService.UpdateAsync(usuario));

        Assert.Contains("El nombre es requerido", exception.Message);
    }

    [Fact]
    public async Task CreateUserWithValidationAsync_WithNullPasswordHash_CreatesUser()
    {
        // Arrange
        var email = "new@example.com";
        var nombre = "New User";
        string? passwordHash = null;
        var rol = "User";

        _mockRepository.Setup(r => r.ExistsByEmailAsync(email))
                      .ReturnsAsync(false);
        
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Usuario>()))
                      .ReturnsAsync((Usuario u) => { u.Id = 1; return u; });

        // Act
        var result = await _usuarioService.CreateUserWithValidationAsync(email, nombre, passwordHash, rol);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
        Assert.Equal(nombre, result.Nombre);
        Assert.Null(result.PasswordHash);
        Assert.Equal(rol, result.Rol);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public async Task GetByRolAsync_WithEmptyResults_ReturnsEmptyCollection()
    {
        // Arrange
        var rol = "NonExistentRole";
        var emptyList = new List<Usuario>();

        _mockRepository.Setup(r => r.GetByRolAsync(rol))
                      .ReturnsAsync(emptyList);

        // Act
        var result = await _usuarioService.GetByRolAsync(rol);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockRepository.Verify(r => r.GetByRolAsync(rol), Times.Once);
    }
}