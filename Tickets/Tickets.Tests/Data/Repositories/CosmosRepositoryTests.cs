using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;
using Tickets.Data.Repositories;
using Tickets.Domain.Entities;
using Xunit;

namespace Tickets.Tests.Data.Repositories;

public class CosmosRepositoryTests
{
    private readonly Mock<Container> _mockContainer;
    private readonly Mock<ILogger<CosmosRepository<TestEntity>>> _mockLogger;
    private readonly CosmosRepository<TestEntity> _repository;

    public CosmosRepositoryTests()
    {
        _mockContainer = new Mock<Container>();
        _mockLogger = new Mock<ILogger<CosmosRepository<TestEntity>>>();
        _repository = new CosmosRepository<TestEntity>(_mockContainer.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEntity_WhenExists()
    {
        // Arrange
        var id = "entity-123";
        var partitionKey = "partition-456";
        var entity = CreateTestEntity(id, partitionKey);

        _mockContainer
            .Setup(c => c.ReadItemAsync<TestEntity>(
                id,
                It.Is<PartitionKey>(pk => pk.ToString().Contains(partitionKey)),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateItemResponse(entity));

        // Act
        var result = await _repository.GetByIdAsync(id, partitionKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal(partitionKey, result.PartitionKey);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var id = "entity-123";
        var partitionKey = "partition-456";

        _mockContainer
            .Setup(c => c.ReadItemAsync<TestEntity>(
                id,
                It.Is<PartitionKey>(pk => pk.ToString().Contains(partitionKey)),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosException("Not found", System.Net.HttpStatusCode.NotFound, 0, "", 0));

        // Act
        var result = await _repository.GetByIdAsync(id, partitionKey);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsException_OnOtherErrors()
    {
        // Arrange
        var id = "entity-123";
        var partitionKey = "partition-456";

        _mockContainer
            .Setup(c => c.ReadItemAsync<TestEntity>(
                id,
                It.Is<PartitionKey>(pk => pk.ToString().Contains(partitionKey)),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosException("Internal error", System.Net.HttpStatusCode.InternalServerError, 0, "", 0));

        // Act & Assert
        await Assert.ThrowsAsync<CosmosException>(() => _repository.GetByIdAsync(id, partitionKey));
    }

    [Fact]
    public async Task CreateAsync_CreatesEntity_WithCorrectMetadata()
    {
        // Arrange
        var entity = CreateTestEntity("entity-123", "partition-456");
        var beforeCreation = DateTime.UtcNow;

        _mockContainer
            .Setup(c => c.CreateItemAsync(
                It.IsAny<TestEntity>(),
                It.Is<PartitionKey>(pk => pk.ToString().Contains(entity.PartitionKey)),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestEntity e, PartitionKey pk, ItemRequestOptions options, CancellationToken ct) =>
            {
                return CreateItemResponse(e);
            });

        // Act
        var result = await _repository.CreateAsync(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestEntity", result.EntityType);
        Assert.True(result.CreatedAt >= beforeCreation);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);

        _mockContainer.Verify(c => c.CreateItemAsync(
            It.Is<TestEntity>(e => e.EntityType == "TestEntity" && e.CreatedAt >= beforeCreation),
            It.IsAny<PartitionKey>(),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEntity_WithUpdatedAtTimestamp()
    {
        // Arrange
        var entity = CreateTestEntity("entity-123", "partition-456");
        var beforeUpdate = DateTime.UtcNow;

        _mockContainer
            .Setup(c => c.ReplaceItemAsync(
                It.IsAny<TestEntity>(),
                entity.Id,
                It.Is<PartitionKey>(pk => pk.ToString().Contains(entity.PartitionKey)),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestEntity e, string id, PartitionKey pk, ItemRequestOptions options, CancellationToken ct) =>
            {
                return CreateItemResponse(e);
            });

        // Act
        var result = await _repository.UpdateAsync(entity);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.UpdatedAt);
        Assert.True(result.UpdatedAt >= beforeUpdate);
        Assert.True(result.UpdatedAt <= DateTime.UtcNow);

        _mockContainer.Verify(c => c.ReplaceItemAsync(
            It.Is<TestEntity>(e => e.UpdatedAt != null && e.UpdatedAt >= beforeUpdate),
            entity.Id,
            It.IsAny<PartitionKey>(),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpsertAsync_UpsertsEntity_WithCorrectMetadata()
    {
        // Arrange
        var entity = CreateTestEntity("entity-123", "partition-456");
        var beforeUpsert = DateTime.UtcNow;

        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<TestEntity>(),
                It.Is<PartitionKey>(pk => pk.ToString().Contains(entity.PartitionKey)),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestEntity e, PartitionKey pk, ItemRequestOptions options, CancellationToken ct) =>
            {
                return CreateItemResponse(e);
            });

        // Act
        var result = await _repository.UpsertAsync(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestEntity", result.EntityType);
        Assert.NotNull(result.UpdatedAt);
        Assert.True(result.UpdatedAt >= beforeUpsert);

        _mockContainer.Verify(c => c.UpsertItemAsync(
            It.Is<TestEntity>(e => e.EntityType == "TestEntity" && e.UpdatedAt != null),
            It.IsAny<PartitionKey>(),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenEntityDeleted()
    {
        // Arrange
        var id = "entity-123";
        var partitionKey = "partition-456";

        _mockContainer
            .Setup(c => c.DeleteItemAsync<TestEntity>(
                id,
                It.Is<PartitionKey>(pk => pk.ToString().Contains(partitionKey)),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateItemResponse<TestEntity>(null!));

        // Act
        var result = await _repository.DeleteAsync(id, partitionKey);

        // Assert
        Assert.True(result);
        _mockContainer.Verify(c => c.DeleteItemAsync<TestEntity>(
            id,
            It.IsAny<PartitionKey>(),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenEntityNotFound()
    {
        // Arrange
        var id = "entity-123";
        var partitionKey = "partition-456";

        _mockContainer
            .Setup(c => c.DeleteItemAsync<TestEntity>(
                id,
                It.Is<PartitionKey>(pk => pk.ToString().Contains(partitionKey)),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosException("Not found", System.Net.HttpStatusCode.NotFound, 0, "", 0));

        // Act
        var result = await _repository.DeleteAsync(id, partitionKey);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateBulkAsync_CreatesMultipleEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            CreateTestEntity("entity-1", "partition-1"),
            CreateTestEntity("entity-2", "partition-2"),
            CreateTestEntity("entity-3", "partition-3")
        };

        _mockContainer
            .Setup(c => c.CreateItemAsync(
                It.IsAny<TestEntity>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestEntity e, PartitionKey pk, ItemRequestOptions options, CancellationToken ct) =>
            {
                return CreateItemResponse(e);
            });

        // Act
        var results = await _repository.CreateBulkAsync(entities);

        // Assert
        var resultList = results.ToList();
        Assert.Equal(3, resultList.Count);
        Assert.All(resultList, e => Assert.Equal("TestEntity", e.EntityType));

        _mockContainer.Verify(c => c.CreateItemAsync(
            It.IsAny<TestEntity>(),
            It.IsAny<PartitionKey>(),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    // Helper classes and methods
    private static TestEntity CreateTestEntity(string id, string partitionKey)
    {
        return new TestEntity
        {
            Id = id,
            TestPartitionKey = partitionKey,
            EntityType = "TestEntity",
            Name = $"Test {id}"
        };
    }

    private static ItemResponse<T> CreateItemResponse<T>(T entity)
    {
        var mockResponse = new Mock<ItemResponse<T>>();
        mockResponse.Setup(r => r.Resource).Returns(entity);
        mockResponse.Setup(r => r.RequestCharge).Returns(5.0);
        return mockResponse.Object;
    }
}

// Test entity class for testing the generic repository
public class TestEntity : BaseEntity
{
    public string TestPartitionKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public override string PartitionKey => TestPartitionKey;
}
