using Parhelion.Infrastructure.Repositories;
using Parhelion.Tests.Fixtures;
using Xunit;

namespace Parhelion.Tests.Unit.Repositories;

/// <summary>
/// Tests para GenericRepository.
/// </summary>
public class GenericRepositoryTests : IClassFixture<InMemoryDbFixture>
{
    private readonly InMemoryDbFixture _fixture;

    public GenericRepositoryTests(InMemoryDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_NewEntity_SetsIdAndCreatedAt()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new GenericRepository<Domain.Entities.Tenant>(context);
        var tenant = TestDataBuilder.CreateTenant("New Tenant");
        tenant.Id = Guid.Empty; // Simular entidad sin ID

        // Act
        var result = await repository.AddAsync(tenant);
        await context.SaveChangesAsync();

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(result.CreatedAt != default);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEntity_ReturnsEntity()
    {
        // Arrange
        using var context = _fixture.CreateSeededContext();
        var repository = new GenericRepository<Domain.Entities.Tenant>(context);
        var expectedId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        // Act
        var result = await repository.GetByIdAsync(expectedId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Company", result.CompanyName);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingEntity_ReturnsNull()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new GenericRepository<Domain.Entities.Tenant>(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_Entity_SetsSoftDeleteFlags()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new GenericRepository<Domain.Entities.Tenant>(context);
        var tenant = TestDataBuilder.CreateTenant("To Delete");
        await repository.AddAsync(tenant);
        await context.SaveChangesAsync();

        // Act
        repository.Delete(tenant);
        await context.SaveChangesAsync();

        // Assert
        Assert.True(tenant.IsDeleted);
        Assert.NotNull(tenant.DeletedAt);
    }

    [Fact]
    public async Task Update_Entity_SetsUpdatedAt()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new GenericRepository<Domain.Entities.Tenant>(context);
        var tenant = TestDataBuilder.CreateTenant("To Update");
        await repository.AddAsync(tenant);
        await context.SaveChangesAsync();

        // Act
        tenant.CompanyName = "Updated Name";
        repository.Update(tenant);
        await context.SaveChangesAsync();

        // Assert
        Assert.NotNull(tenant.UpdatedAt);
        Assert.Equal("Updated Name", tenant.CompanyName);
    }

    [Fact]
    public async Task AnyAsync_WithMatchingPredicate_ReturnsTrue()
    {
        // Arrange
        using var context = _fixture.CreateSeededContext();
        var repository = new GenericRepository<Domain.Entities.Tenant>(context);

        // Act
        var exists = await repository.AnyAsync(t => t.CompanyName == "Test Company");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task AnyAsync_WithNonMatchingPredicate_ReturnsFalse()
    {
        // Arrange
        using var context = _fixture.CreateSeededContext();
        var repository = new GenericRepository<Domain.Entities.Tenant>(context);

        // Act
        var exists = await repository.AnyAsync(t => t.CompanyName == "Non Existing");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        // Arrange
        using var context = _fixture.CreateSeededContext();
        var repository = new GenericRepository<Domain.Entities.Role>(context);

        // Act
        var count = await repository.CountAsync();

        // Assert
        Assert.Equal(3, count); // Admin, Driver, Warehouse from seed
    }

    [Fact]
    public async Task FindAsync_WithPredicate_ReturnsMatchingEntities()
    {
        // Arrange
        using var context = _fixture.CreateSeededContext();
        var repository = new GenericRepository<Domain.Entities.Role>(context);

        // Act
        var results = await repository.FindAsync(r => r.Name.Contains("Admin"));

        // Assert
        Assert.Single(results);
        Assert.Equal("Admin", results.First().Name);
    }
}
