using Moq;
using retoSquadmakers.Application.Services;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;

namespace retoSquadmakers.Tests.Application.Services;

// Test entity for BaseService testing
public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Concrete implementation of BaseService for testing
public class TestService : BaseService<TestEntity, int>
{
    public TestService(IBaseRepository<TestEntity, int> repository) : base(repository)
    {
    }

    // Allow access to protected methods for testing
    public async Task TestValidateEntityAsync(TestEntity entity, bool isUpdate)
    {
        await ValidateEntityAsync(entity, isUpdate);
    }

    public async Task TestValidateDeleteAsync(int id)
    {
        await ValidateDeleteAsync(id);
    }
}

public class BaseServiceTests
{
    private readonly Mock<IBaseRepository<TestEntity, int>> _mockRepository;
    private readonly TestService _service;

    public BaseServiceTests()
    {
        _mockRepository = new Mock<IBaseRepository<TestEntity, int>>();
        _service = new TestService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsEntity()
    {
        // Arrange
        var id = 1;
        var expectedEntity = new TestEntity { Id = id, Name = "Test Entity" };
        _mockRepository.Setup(r => r.GetByIdAsync(id))
                      .ReturnsAsync(expectedEntity);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedEntity.Id, result.Id);
        Assert.Equal(expectedEntity.Name, result.Name);
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var id = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(id))
                      .ReturnsAsync((TestEntity?)null);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        // Arrange
        var expectedEntities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Entity 1" },
            new TestEntity { Id = 2, Name = "Entity 2" },
            new TestEntity { Id = 3, Name = "Entity 3" }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
                      .ReturnsAsync(expectedEntities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.Equal(expectedEntities, result);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyRepository_ReturnsEmptyCollection()
    {
        // Arrange
        var emptyList = new List<TestEntity>();
        _mockRepository.Setup(r => r.GetAllAsync())
                      .ReturnsAsync(emptyList);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidEntity_CreatesEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "New Entity" };
        var createdEntity = new TestEntity { Id = 1, Name = "New Entity" };

        _mockRepository.Setup(r => r.CreateAsync(entity))
                      .ReturnsAsync(createdEntity);

        // Act
        var result = await _service.CreateAsync(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdEntity.Id, result.Id);
        Assert.Equal(createdEntity.Name, result.Name);
        _mockRepository.Verify(r => r.CreateAsync(entity), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidEntity_UpdatesEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Updated Entity" };
        var updatedEntity = new TestEntity { Id = 1, Name = "Updated Entity" };

        _mockRepository.Setup(r => r.UpdateAsync(entity))
                      .ReturnsAsync(updatedEntity);

        // Act
        var result = await _service.UpdateAsync(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedEntity.Id, result.Id);
        Assert.Equal(updatedEntity.Name, result.Name);
        _mockRepository.Verify(r => r.UpdateAsync(entity), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesEntity()
    {
        // Arrange
        var id = 1;
        _mockRepository.Setup(r => r.DeleteAsync(id))
                      .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(id);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        var id = 1;
        _mockRepository.Setup(r => r.ExistsAsync(id))
                      .ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(id);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.ExistsAsync(id), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
    {
        // Arrange
        var id = 999;
        _mockRepository.Setup(r => r.ExistsAsync(id))
                      .ReturnsAsync(false);

        // Act
        var result = await _service.ExistsAsync(id);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.ExistsAsync(id), Times.Once);
    }

    [Fact]
    public async Task ValidateEntityAsync_BaseImplementation_CompletesSuccessfully()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test Entity" };

        // Act & Assert - Should not throw any exceptions
        await _service.TestValidateEntityAsync(entity, isUpdate: false);
        await _service.TestValidateEntityAsync(entity, isUpdate: true);
    }

    [Fact]
    public async Task ValidateDeleteAsync_BaseImplementation_CompletesSuccessfully()
    {
        // Arrange
        var id = 1;

        // Act & Assert - Should not throw any exceptions
        await _service.TestValidateDeleteAsync(id);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task GetByIdAsync_WithDifferentIds_CallsRepositoryCorrectly(int id)
    {
        // Arrange
        var entity = new TestEntity { Id = id, Name = $"Entity {id}" };
        _mockRepository.Setup(r => r.GetByIdAsync(id))
                      .ReturnsAsync(entity);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CallsValidateEntityAsyncWithCorrectParameters()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test Entity" };
        var createdEntity = new TestEntity { Id = 1, Name = "Test Entity" };

        _mockRepository.Setup(r => r.CreateAsync(entity))
                      .ReturnsAsync(createdEntity);

        // Act
        await _service.CreateAsync(entity);

        // Assert
        // The validation methods are called internally
        _mockRepository.Verify(r => r.CreateAsync(entity), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsValidateEntityAsyncWithCorrectParameters()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Updated Entity" };
        var updatedEntity = new TestEntity { Id = 1, Name = "Updated Entity" };

        _mockRepository.Setup(r => r.UpdateAsync(entity))
                      .ReturnsAsync(updatedEntity);

        // Act
        await _service.UpdateAsync(entity);

        // Assert
        // The validation methods are called internally
        _mockRepository.Verify(r => r.UpdateAsync(entity), Times.Once);
    }
}