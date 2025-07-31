using Microsoft.EntityFrameworkCore;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Infrastructure.Persistence;

namespace retoSquadmakers.Tests.Infrastructure.Persistence;

// Test entity for BaseRepository testing
public class TestRepositoryEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Test context for testing
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<TestRepositoryEntity> TestEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestRepositoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });
    }
}

// Concrete repository implementation for testing
public class TestRepository : BaseRepository<TestRepositoryEntity, int>
{
    public TestRepository(ApplicationDbContext context) : base(context)
    {
    }
}

public class BaseRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly TestRepository _repository;

    public BaseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new TestRepository(_context);

        // Ensure database is created
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsEntity()
    {
        // Arrange
        var entity = new Usuario
        {
            Email = "test@example.com",
            Nombre = "Test User",
            PasswordHash = "hash",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(entity);
        await _context.SaveChangesAsync();

        var usuarioRepo = new UsuarioRepository(_context);

        // Act
        var result = await usuarioRepo.GetByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Email, result.Email);
        Assert.Equal(entity.Nombre, result.Nombre);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        var usuarioRepo = new UsuarioRepository(_context);

        // Act
        var result = await usuarioRepo.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleEntities_ReturnsAllEntities()
    {
        // Arrange
        var usuarios = new List<Usuario>
        {
            new Usuario { Email = "user1@test.com", Nombre = "User 1", Rol = "User", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "user2@test.com", Nombre = "User 2", Rol = "User", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "user3@test.com", Nombre = "User 3", Rol = "Admin", FechaCreacion = DateTime.UtcNow }
        };

        _context.Usuarios.AddRange(usuarios);
        await _context.SaveChangesAsync();

        var usuarioRepo = new UsuarioRepository(_context);

        // Act
        var result = await usuarioRepo.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.Contains(result, u => u.Email == "user1@test.com");
        Assert.Contains(result, u => u.Email == "user2@test.com");
        Assert.Contains(result, u => u.Email == "user3@test.com");
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyDatabase_ReturnsEmptyCollection()
    {
        // Arrange
        var usuarioRepo = new UsuarioRepository(_context);

        // Act
        var result = await usuarioRepo.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidEntity_CreatesAndReturnsEntity()
    {
        // Arrange
        var usuario = new Usuario
        {
            Email = "newuser@test.com",
            Nombre = "New User",
            PasswordHash = "hash123",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        var usuarioRepo = new UsuarioRepository(_context);

        // Act
        var result = await usuarioRepo.CreateAsync(usuario);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(usuario.Email, result.Email);
        Assert.Equal(usuario.Nombre, result.Nombre);

        // Verify it's actually in the database
        var dbEntity = await _context.Usuarios.FindAsync(result.Id);
        Assert.NotNull(dbEntity);
        Assert.Equal(usuario.Email, dbEntity.Email);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingEntity_UpdatesAndReturnsEntity()
    {
        // Arrange
        var usuario = new Usuario
        {
            Email = "original@test.com",
            Nombre = "Original Name",
            PasswordHash = "hash",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        // Modify the entity
        usuario.Nombre = "Updated Name";
        usuario.Email = "updated@test.com";

        var usuarioRepo = new UsuarioRepository(_context);

        // Act
        var result = await usuarioRepo.UpdateAsync(usuario);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Nombre);
        Assert.Equal("updated@test.com", result.Email);

        // Verify it's updated in the database
        var dbEntity = await _context.Usuarios.FindAsync(usuario.Id);
        Assert.NotNull(dbEntity);
        Assert.Equal("Updated Name", dbEntity.Nombre);
        Assert.Equal("updated@test.com", dbEntity.Email);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingEntity_RemovesEntityFromDatabase()
    {
        // Arrange
        var usuario = new Usuario
        {
            Email = "todelete@test.com",
            Nombre = "To Delete",
            PasswordHash = "hash",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        var entityId = usuario.Id;

        var usuarioRepo = new UsuarioRepository(_context);

        // Act
        await usuarioRepo.DeleteAsync(entityId);

        // Assert
        var deletedEntity = await _context.Usuarios.FindAsync(entityId);
        Assert.Null(deletedEntity);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_DoesNotThrowException()
    {
        // Arrange
        var usuarioRepo = new UsuarioRepository(_context);

        // Act & Assert - Should not throw exception
        await usuarioRepo.DeleteAsync(999);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingEntity_ReturnsTrue()
    {
        // Arrange
        var usuario = new Usuario
        {
            Email = "exists@test.com",
            Nombre = "Exists User",
            PasswordHash = "hash",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var usuarioRepo = new UsuarioRepository(_context);

        // Act
        var result = await usuarioRepo.ExistsAsync(usuario.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingEntity_ReturnsFalse()
    {
        // Arrange
        var usuarioRepo = new UsuarioRepository(_context);

        // Act
        var result = await usuarioRepo.ExistsAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CountAsync_WithMultipleEntities_ReturnsCorrectCount()
    {
        // Arrange
        var usuarios = new List<Usuario>
        {
            new Usuario { Email = "count1@test.com", Nombre = "Count 1", Rol = "User", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "count2@test.com", Nombre = "Count 2", Rol = "User", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "count3@test.com", Nombre = "Count 3", Rol = "Admin", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "count4@test.com", Nombre = "Count 4", Rol = "User", FechaCreacion = DateTime.UtcNow }
        };

        _context.Usuarios.AddRange(usuarios);
        await _context.SaveChangesAsync();

        var usuarioRepo = new UsuarioRepository(_context);

        // Act
        var result = await usuarioRepo.CountAsync();

        // Assert
        Assert.Equal(4, result);
    }

    [Fact]
    public async Task CountAsync_WithEmptyDatabase_ReturnsZero()
    {
        // Arrange
        var usuarioRepo = new UsuarioRepository(_context);

        // Act
        var result = await usuarioRepo.CountAsync();

        // Assert
        Assert.Equal(0, result);
    }
}