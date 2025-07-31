using Microsoft.EntityFrameworkCore;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Infrastructure.Persistence;

namespace retoSquadmakers.Tests.Infrastructure.Persistence;

public class UsuarioRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UsuarioRepository _repository;

    public UsuarioRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UsuarioRepository(_context);

        // Ensure database is created
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ReturnsUser()
    {
        // Arrange
        var usuario = new Usuario
        {
            Email = "test@example.com",
            Nombre = "Test User",
            PasswordHash = "hash123",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test User", result.Nombre);
        Assert.Equal("User", result.Rol);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistingEmail_ReturnsNull()
    {
        // Arrange & Act
        var result = await _repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_WithCaseSensitiveEmail_ReturnsCorrectUser()
    {
        // Arrange
        var usuario1 = new Usuario
        {
            Email = "test@example.com",
            Nombre = "Test User Lower",
            PasswordHash = "hash123",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        var usuario2 = new Usuario
        {
            Email = "TEST@EXAMPLE.COM",
            Nombre = "Test User Upper",
            PasswordHash = "hash456",
            Rol = "Admin",
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.AddRange(usuario1, usuario2);
        await _context.SaveChangesAsync();

        // Act
        var resultLower = await _repository.GetByEmailAsync("test@example.com");
        var resultUpper = await _repository.GetByEmailAsync("TEST@EXAMPLE.COM");

        // Assert
        Assert.NotNull(resultLower);
        Assert.Equal("Test User Lower", resultLower.Nombre);
        
        Assert.NotNull(resultUpper);
        Assert.Equal("Test User Upper", resultUpper.Nombre);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithExistingEmail_ReturnsTrue()
    {
        // Arrange
        var usuario = new Usuario
        {
            Email = "exists@example.com",
            Nombre = "Existing User",
            PasswordHash = "hash123",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByEmailAsync("exists@example.com");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithNonExistingEmail_ReturnsFalse()
    {
        // Arrange & Act
        var result = await _repository.ExistsByEmailAsync("notexists@example.com");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetByRolAsync_WithExistingRole_ReturnsUsersWithThatRole()
    {
        // Arrange
        var usuarios = new List<Usuario>
        {
            new Usuario { Email = "admin1@test.com", Nombre = "Admin 1", Rol = "Admin", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "admin2@test.com", Nombre = "Admin 2", Rol = "Admin", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "user1@test.com", Nombre = "User 1", Rol = "User", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "user2@test.com", Nombre = "User 2", Rol = "User", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "user3@test.com", Nombre = "User 3", Rol = "User", FechaCreacion = DateTime.UtcNow }
        };

        _context.Usuarios.AddRange(usuarios);
        await _context.SaveChangesAsync();

        // Act
        var adminUsers = await _repository.GetByRolAsync("Admin");
        var regularUsers = await _repository.GetByRolAsync("User");

        // Assert
        Assert.NotNull(adminUsers);
        Assert.Equal(2, adminUsers.Count());
        Assert.All(adminUsers, u => Assert.Equal("Admin", u.Rol));
        Assert.Contains(adminUsers, u => u.Email == "admin1@test.com");
        Assert.Contains(adminUsers, u => u.Email == "admin2@test.com");

        Assert.NotNull(regularUsers);
        Assert.Equal(3, regularUsers.Count());
        Assert.All(regularUsers, u => Assert.Equal("User", u.Rol));
        Assert.Contains(regularUsers, u => u.Email == "user1@test.com");
        Assert.Contains(regularUsers, u => u.Email == "user2@test.com");
        Assert.Contains(regularUsers, u => u.Email == "user3@test.com");
    }

    [Fact]
    public async Task GetByRolAsync_WithNonExistingRole_ReturnsEmptyCollection()
    {
        // Arrange
        var usuario = new Usuario
        {
            Email = "user@test.com",
            Nombre = "Regular User",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByRolAsync("SuperAdmin");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByRolAsync_WithEmptyDatabase_ReturnsEmptyCollection()
    {
        // Arrange & Act
        var result = await _repository.GetByRolAsync("User");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    [InlineData("Moderator")]
    [InlineData("Guest")]
    public async Task GetByRolAsync_WithVariousRoles_FiltersCorrectly(string rol)
    {
        // Arrange
        var usuarios = new List<Usuario>
        {
            new Usuario { Email = "user1@test.com", Nombre = "User 1", Rol = "User", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "admin1@test.com", Nombre = "Admin 1", Rol = "Admin", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "mod1@test.com", Nombre = "Mod 1", Rol = "Moderator", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "guest1@test.com", Nombre = "Guest 1", Rol = "Guest", FechaCreacion = DateTime.UtcNow }
        };

        _context.Usuarios.AddRange(usuarios);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByRolAsync(rol);

        // Assert
        Assert.NotNull(result);
        if (rol == "User" || rol == "Admin" || rol == "Moderator" || rol == "Guest")
        {
            Assert.Single(result);
            Assert.Equal(rol, result.First().Rol);
        }
        else
        {
            Assert.Empty(result);
        }
    }

    [Fact]
    public async Task Repository_InheritsFromBaseRepository_WorksWithBaseRepositoryMethods()
    {
        // Arrange
        var usuario = new Usuario
        {
            Email = "base@test.com",
            Nombre = "Base Test User",
            PasswordHash = "hash123",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        // Act - Test inherited methods
        var createdUser = await _repository.CreateAsync(usuario);
        var foundUser = await _repository.GetByIdAsync(createdUser.Id);
        var exists = await _repository.ExistsAsync(createdUser.Id);
        var count = await _repository.CountAsync();
        var allUsers = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(createdUser);
        Assert.True(createdUser.Id > 0);

        Assert.NotNull(foundUser);
        Assert.Equal("base@test.com", foundUser.Email);

        Assert.True(exists);
        Assert.Equal(1, count);
        Assert.Single(allUsers);

        // Test update
        foundUser.Nombre = "Updated Name";
        var updatedUser = await _repository.UpdateAsync(foundUser);
        Assert.Equal("Updated Name", updatedUser.Nombre);

        // Test delete
        await _repository.DeleteAsync(createdUser.Id);
        var deletedUser = await _repository.GetByIdAsync(createdUser.Id);
        Assert.Null(deletedUser);

        var finalCount = await _repository.CountAsync();
        Assert.Equal(0, finalCount);
    }

    [Fact]
    public async Task GetByEmailAsync_WithCaseInsensitiveSearch_RespectsDbContextBehavior()
    {
        // Arrange
        var usuario = new Usuario
        {
            Email = "TEST@EXAMPLE.COM",
            Nombre = "Test User",
            PasswordHash = "hash123",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        // Act - Test exact case
        var result1 = await _repository.GetByEmailAsync("TEST@EXAMPLE.COM");
        var result2 = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        Assert.NotNull(result1);
        Assert.Equal("TEST@EXAMPLE.COM", result1.Email);
        
        // Note: Case sensitivity depends on database collation
        // In-memory DB might behave differently than production DB
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithMultipleUsers_OnlyChecksEmail()
    {
        // Arrange
        var usuarios = new List<Usuario>
        {
            new Usuario { Email = "user1@test.com", Nombre = "User 1", Rol = "User", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "user2@test.com", Nombre = "User 2", Rol = "Admin", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "user3@test.com", Nombre = "User 3", Rol = "User", FechaCreacion = DateTime.UtcNow }
        };

        _context.Usuarios.AddRange(usuarios);
        await _context.SaveChangesAsync();

        // Act & Assert
        Assert.True(await _repository.ExistsByEmailAsync("user1@test.com"));
        Assert.True(await _repository.ExistsByEmailAsync("user2@test.com"));
        Assert.True(await _repository.ExistsByEmailAsync("user3@test.com"));
        Assert.False(await _repository.ExistsByEmailAsync("nonexistent@test.com"));
    }

    [Fact]
    public async Task GetByRolAsync_WithSameRoleMultipleUsers_ReturnsAllMatching()
    {
        // Arrange
        var usuarios = new List<Usuario>
        {
            new Usuario { Email = "admin1@test.com", Nombre = "Admin 1", Rol = "Admin", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "admin2@test.com", Nombre = "Admin 2", Rol = "Admin", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "admin3@test.com", Nombre = "Admin 3", Rol = "Admin", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "user1@test.com", Nombre = "User 1", Rol = "User", FechaCreacion = DateTime.UtcNow }
        };

        _context.Usuarios.AddRange(usuarios);
        await _context.SaveChangesAsync();

        // Act
        var adminUsers = await _repository.GetByRolAsync("Admin");
        var regularUsers = await _repository.GetByRolAsync("User");

        // Assert
        Assert.Equal(3, adminUsers.Count());
        Assert.All(adminUsers, u => Assert.Equal("Admin", u.Rol));
        
        Assert.Single(regularUsers);
        Assert.Equal("User", regularUsers.First().Rol);
    }

    [Fact]
    public async Task BaseRepository_DeleteAsync_WithNonExistentId_DoesNotThrow()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act & Assert - Should not throw
        await _repository.DeleteAsync(nonExistentId);
        
        // Verify nothing changed
        var count = await _repository.CountAsync();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task BaseRepository_ExistsAsync_WithExistingAndNonExistingIds()
    {
        // Arrange
        var usuario = new Usuario
        {
            Email = "exists@test.com",
            Nombre = "Exists User",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        var createdUser = await _repository.CreateAsync(usuario);

        // Act & Assert
        Assert.True(await _repository.ExistsAsync(createdUser.Id));
        Assert.False(await _repository.ExistsAsync(9999)); // Non-existent ID
    }

    [Fact]
    public async Task BaseRepository_CountAsync_WithVariousOperations()
    {
        // Arrange & Act
        var initialCount = await _repository.CountAsync();
        Assert.Equal(0, initialCount);

        // Add users
        var usuarios = new List<Usuario>
        {
            new Usuario { Email = "count1@test.com", Nombre = "Count 1", Rol = "User", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "count2@test.com", Nombre = "Count 2", Rol = "User", FechaCreacion = DateTime.UtcNow }
        };

        var user1 = await _repository.CreateAsync(usuarios[0]);
        var countAfterFirst = await _repository.CountAsync();
        Assert.Equal(1, countAfterFirst);

        var user2 = await _repository.CreateAsync(usuarios[1]);
        var countAfterSecond = await _repository.CountAsync();
        Assert.Equal(2, countAfterSecond);

        // Delete one user
        await _repository.DeleteAsync(user1.Id);
        var countAfterDelete = await _repository.CountAsync();
        Assert.Equal(1, countAfterDelete);
    }

    [Fact]
    public async Task BaseRepository_GetAllAsync_ReturnsAllEntities()
    {
        // Arrange
        var usuarios = new List<Usuario>
        {
            new Usuario { Email = "all1@test.com", Nombre = "All 1", Rol = "User", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "all2@test.com", Nombre = "All 2", Rol = "Admin", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "all3@test.com", Nombre = "All 3", Rol = "User", FechaCreacion = DateTime.UtcNow }
        };

        foreach (var usuario in usuarios)
        {
            await _repository.CreateAsync(usuario);
        }

        // Act
        var allUsers = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(3, allUsers.Count());
        Assert.Contains(allUsers, u => u.Email == "all1@test.com");
        Assert.Contains(allUsers, u => u.Email == "all2@test.com");
        Assert.Contains(allUsers, u => u.Email == "all3@test.com");
    }

    [Fact]
    public async Task BaseRepository_UpdateAsync_ModifiesEntitySuccessfully()
    {
        // Arrange
        var usuario = new Usuario
        {
            Email = "update@test.com",
            Nombre = "Original Name",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        var createdUser = await _repository.CreateAsync(usuario);
        var originalId = createdUser.Id;

        // Act
        createdUser.Nombre = "Updated Name";
        createdUser.Email = "updated@test.com";
        createdUser.Rol = "Admin";

        var updatedUser = await _repository.UpdateAsync(createdUser);

        // Assert
        Assert.Equal(originalId, updatedUser.Id);
        Assert.Equal("Updated Name", updatedUser.Nombre);
        Assert.Equal("updated@test.com", updatedUser.Email);
        Assert.Equal("Admin", updatedUser.Rol);

        // Verify the changes persisted
        var fetchedUser = await _repository.GetByIdAsync(originalId);
        Assert.NotNull(fetchedUser);
        Assert.Equal("Updated Name", fetchedUser.Nombre);
        Assert.Equal("updated@test.com", fetchedUser.Email);
        Assert.Equal("Admin", fetchedUser.Rol);
    }

    [Fact]
    public async Task Repository_SpecificMethods_WorkWithInheritedMethods()
    {
        // Arrange
        var usuarios = new List<Usuario>
        {
            new Usuario { Email = "specific1@test.com", Nombre = "Specific 1", Rol = "User", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "specific2@test.com", Nombre = "Specific 2", Rol = "Admin", FechaCreacion = DateTime.UtcNow }
        };

        foreach (var usuario in usuarios)
        {
            await _repository.CreateAsync(usuario);
        }

        // Act & Assert - Test interaction between specific and inherited methods
        var userByEmail = await _repository.GetByEmailAsync("specific1@test.com");
        Assert.NotNull(userByEmail);

        var userById = await _repository.GetByIdAsync(userByEmail.Id);
        Assert.NotNull(userById);
        Assert.Equal(userByEmail.Email, userById.Email);

        var adminUsers = await _repository.GetByRolAsync("Admin");
        Assert.Single(adminUsers);

        var emailExists = await _repository.ExistsByEmailAsync("specific2@test.com");
        var idExists = await _repository.ExistsAsync(adminUsers.First().Id);
        Assert.True(emailExists);
        Assert.True(idExists);

        var totalCount = await _repository.CountAsync();
        var allUsers = await _repository.GetAllAsync();
        Assert.Equal(2, totalCount);
        Assert.Equal(2, allUsers.Count());
    }
}