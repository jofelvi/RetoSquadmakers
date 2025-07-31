using Microsoft.EntityFrameworkCore;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Infrastructure.Persistence;

namespace retoSquadmakers.Tests.Infrastructure.Persistence;

public class ChisteRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ChisteRepository _repository;

    public ChisteRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new ChisteRepository(_context);

        // Ensure database is created
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetByAutorIdAsync_WithExistingAutor_ReturnsChistes()
    {
        // Arrange
        var autor = new Usuario
        {
            Email = "autor@example.com",
            Nombre = "Autor Test",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(autor);
        await _context.SaveChangesAsync();

        var chistes = new List<Chiste>
        {
            new Chiste { Texto = "Chiste 1 del autor", AutorId = autor.Id, Origen = "Local", FechaCreacion = DateTime.UtcNow },
            new Chiste { Texto = "Chiste 2 del autor", AutorId = autor.Id, Origen = "API", FechaCreacion = DateTime.UtcNow },
            new Chiste { Texto = "Chiste de otro autor", AutorId = 999, Origen = "Local", FechaCreacion = DateTime.UtcNow }
        };

        _context.Chistes.AddRange(chistes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByAutorIdAsync(autor.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Equal(autor.Id, c.AutorId));
        Assert.All(result, c => Assert.NotNull(c.Autor));
        Assert.All(result, c => Assert.Equal("Autor Test", c.Autor.Nombre));
    }

    [Fact]
    public async Task GetByAutorIdAsync_WithNonExistingAutor_ReturnsEmpty()
    {
        // Arrange
        var nonExistentAutorId = 999;

        // Act
        var result = await _repository.GetByAutorIdAsync(nonExistentAutorId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FilterAsync_WithMinPalabras_FiltersCorrectly()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _repository.FilterAsync(minPalabras: 4);

        // Assert
        Assert.NotNull(result);
        var chistesList = result.ToList();
        Assert.True(chistesList.All(c => c.Texto.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 4));
    }

    [Fact]
    public async Task FilterAsync_WithContiene_FiltersCorrectly()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _repository.FilterAsync(contiene: "gracioso");

        // Assert
        Assert.NotNull(result);
        Assert.All(result, c => Assert.Contains("gracioso", c.Texto));
    }

    [Fact]
    public async Task FilterAsync_WithAutorId_FiltersCorrectly()
    {
        // Arrange
        await SeedTestData();
        var autor = await _context.Usuarios.FirstAsync();

        // Act
        var result = await _repository.FilterAsync(autorId: autor.Id);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, c => Assert.Equal(autor.Id, c.AutorId));
        Assert.All(result, c => Assert.NotNull(c.Autor));
    }

    [Fact]
    public async Task FilterAsync_WithTematicaId_FiltersCorrectly()
    {
        // Arrange
        await SeedTestDataWithTematicas();
        var tematica = await _context.Tematicas.FirstAsync();

        // Act
        var result = await _repository.FilterAsync(tematicaId: tematica.Id);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, c => Assert.True(c.ChisteTematicas.Any(ct => ct.TematicaId == tematica.Id)));
    }

    [Fact]
    public async Task FilterAsync_WithMultipleFilters_FiltersCorrectly()
    {
        // Arrange
        await SeedTestDataWithTematicas();
        var autor = await _context.Usuarios.FirstAsync();
        var tematica = await _context.Tematicas.FirstAsync();

        // Act
        var result = await _repository.FilterAsync(
            minPalabras: 3,
            contiene: "Test",
            autorId: autor.Id,
            tematicaId: tematica.Id);

        // Assert
        Assert.NotNull(result);
        foreach (var chiste in result)
        {
            Assert.True(chiste.Texto.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 3);
            Assert.Contains("Test", chiste.Texto);
            Assert.Equal(autor.Id, chiste.AutorId);
            Assert.True(chiste.ChisteTematicas.Any(ct => ct.TematicaId == tematica.Id));
        }
    }

    [Fact]
    public async Task FilterAsync_WithNoFilters_ReturnsAll()
    {
        // Arrange
        await SeedTestData();
        var totalChistes = await _context.Chistes.CountAsync();

        // Act
        var result = await _repository.FilterAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(totalChistes, result.Count());
        Assert.All(result, c => Assert.NotNull(c.Autor));
    }

    [Fact]
    public async Task GetRandomLocalChistes_WithValidCount_ReturnsLocalChistes()
    {
        // Arrange
        await SeedTestData();
        var count = 3;

        // Act
        var result = await _repository.GetRandomLocalChistes(count);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count() <= count);
        Assert.All(result, c => Assert.Equal("Local", c.Origen));
        Assert.All(result, c => Assert.NotNull(c.Autor));
    }

    [Fact]
    public async Task GetRandomLocalChistes_WithCountGreaterThanAvailable_ReturnsAllAvailable()
    {
        // Arrange
        await SeedTestData();
        var localChistesCount = await _context.Chistes.CountAsync(c => c.Origen == "Local");
        var requestedCount = localChistesCount + 10;

        // Act
        var result = await _repository.GetRandomLocalChistes(requestedCount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(localChistesCount, result.Count());
        Assert.All(result, c => Assert.Equal("Local", c.Origen));
    }

    [Fact]
    public async Task GetRandomLocalChistes_WithNoLocalChistes_ReturnsEmpty()
    {
        // Arrange
        var autor = new Usuario
        {
            Email = "test@example.com",
            Nombre = "Test User",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(autor);
        await _context.SaveChangesAsync();

        // Add only non-local chistes
        var chistes = new List<Chiste>
        {
            new Chiste { Texto = "Chiste API", AutorId = autor.Id, Origen = "API", FechaCreacion = DateTime.UtcNow },
            new Chiste { Texto = "Chiste Importado", AutorId = autor.Id, Origen = "Importado", FechaCreacion = DateTime.UtcNow }
        };

        _context.Chistes.AddRange(chistes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRandomLocalChistes(5);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("Local")]
    [InlineData("API")]
    [InlineData("Importado")]
    public async Task GetByOrigenAsync_WithValidOrigen_ReturnsChistes(string origen)
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _repository.GetByOrigenAsync(origen);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, c => Assert.Equal(origen, c.Origen));
        Assert.All(result, c => Assert.NotNull(c.Autor));
    }

    [Fact]
    public async Task GetByOrigenAsync_WithNonExistentOrigen_ReturnsEmpty()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _repository.GetByOrigenAsync("NoExiste");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_IncludesAuthor()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.All(result, c => Assert.NotNull(c.Autor));
        Assert.All(result, c => Assert.NotEqual(0, c.Autor.Id));
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsFullChisteWithRelations()
    {
        // Arrange
        await SeedTestDataWithTematicas();
        var chisteId = await _context.Chistes.Select(c => c.Id).FirstAsync();

        // Act
        var result = await _repository.GetByIdAsync(chisteId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(chisteId, result.Id);
        Assert.NotNull(result.Autor);
        Assert.NotNull(result.ChisteTematicas);
        
        // Verify tematicas are loaded
        foreach (var chisteTematica in result.ChisteTematicas)
        {
            Assert.NotNull(chisteTematica.Tematica);
            Assert.NotEqual(0, chisteTematica.Tematica.Id);
        }
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Repository_InheritsFromBaseRepository_WorksWithBaseRepositoryMethods()
    {
        // Arrange
        var autor = new Usuario
        {
            Email = "base@test.com",
            Nombre = "Base Test User",
            Rol = "User",
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(autor);
        await _context.SaveChangesAsync();

        var chiste = new Chiste
        {
            Texto = "Chiste para pruebas base",
            AutorId = autor.Id,
            Origen = "Test",
            FechaCreacion = DateTime.UtcNow
        };

        // Act - Test inherited methods
        var createdChiste = await _repository.CreateAsync(chiste);
        var exists = await _repository.ExistsAsync(createdChiste.Id);
        var count = await _repository.CountAsync();

        // Assert
        Assert.NotNull(createdChiste);
        Assert.True(createdChiste.Id > 0);
        Assert.True(exists);
        Assert.Equal(1, count);

        // Test update
        createdChiste.Texto = "Texto actualizado";
        var updatedChiste = await _repository.UpdateAsync(createdChiste);
        Assert.Equal("Texto actualizado", updatedChiste.Texto);

        // Test delete
        await _repository.DeleteAsync(createdChiste.Id);
        var deletedChiste = await _repository.GetByIdAsync(createdChiste.Id);
        Assert.Null(deletedChiste);

        var finalCount = await _repository.CountAsync();
        Assert.Equal(0, finalCount);
    }

    private async Task SeedTestData()
    {
        var usuarios = new List<Usuario>
        {
            new Usuario { Email = "user1@test.com", Nombre = "User 1", Rol = "User", FechaCreacion = DateTime.UtcNow },
            new Usuario { Email = "user2@test.com", Nombre = "User 2", Rol = "User", FechaCreacion = DateTime.UtcNow }
        };

        _context.Usuarios.AddRange(usuarios);
        await _context.SaveChangesAsync();

        var chistes = new List<Chiste>
        {
            new Chiste { Texto = "Un chiste muy gracioso", AutorId = usuarios[0].Id, Origen = "Local", FechaCreacion = DateTime.UtcNow },
            new Chiste { Texto = "Otro chiste gracioso divertido", AutorId = usuarios[0].Id, Origen = "API", FechaCreacion = DateTime.UtcNow },
            new Chiste { Texto = "Chiste corto", AutorId = usuarios[1].Id, Origen = "Local", FechaCreacion = DateTime.UtcNow },
            new Chiste { Texto = "Este es un chiste muy largo y detallado", AutorId = usuarios[1].Id, Origen = "Importado", FechaCreacion = DateTime.UtcNow }
        };

        _context.Chistes.AddRange(chistes);
        await _context.SaveChangesAsync();
    }

    private async Task SeedTestDataWithTematicas()
    {
        await SeedTestData();

        var tematicas = new List<Tematica>
        {
            new Tematica { Nombre = "Humor" },
            new Tematica { Nombre = "Trabajo" }
        };

        _context.Tematicas.AddRange(tematicas);
        await _context.SaveChangesAsync();

        var chiste = await _context.Chistes.FirstAsync();
        var chisteTematicas = new List<ChisteTematica>
        {
            new ChisteTematica { ChisteId = chiste.Id, TematicaId = tematicas[0].Id },
            new ChisteTematica { ChisteId = chiste.Id, TematicaId = tematicas[1].Id }
        };

        _context.ChisteTematicas.AddRange(chisteTematicas);
        await _context.SaveChangesAsync();

        // Add a test chiste with specific content for filtering tests
        var usuario = await _context.Usuarios.FirstAsync();
        var testChiste = new Chiste
        {
            Texto = "Test chiste con muchas palabras para filtrar",
            AutorId = usuario.Id,
            Origen = "Local",
            FechaCreacion = DateTime.UtcNow
        };

        _context.Chistes.Add(testChiste);
        await _context.SaveChangesAsync();

        var testChisteTematica = new ChisteTematica
        {
            ChisteId = testChiste.Id,
            TematicaId = tematicas[0].Id
        };

        _context.ChisteTematicas.Add(testChisteTematica);
        await _context.SaveChangesAsync();
    }
}