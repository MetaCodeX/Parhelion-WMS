using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Application.Interfaces;
using Parhelion.Infrastructure.Services.Shipment;
using Parhelion.Infrastructure.Validators;
using Parhelion.Tests.Fixtures;
using Xunit;

namespace Parhelion.Tests.Unit.Services.Shipment;

/// <summary>
/// Tests para ShipmentService.
/// </summary>
public class ShipmentServiceTests : IClassFixture<ServiceTestFixture>
{
    private readonly ServiceTestFixture _fixture;
    private readonly CargoCompatibilityValidator _cargoValidator = new();
    private readonly IWebhookPublisher _webhookPublisher = new NullWebhookPublisher();

    public ShipmentServiceTests(ServiceTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        // Arrange
        var (uow, ctx) = _fixture.CreateUnitOfWork();
        var service = new ShipmentService(uow, _cargoValidator, _webhookPublisher);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllAsync(request);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        // Arrange
        var (uow, ctx) = _fixture.CreateUnitOfWork();
        var service = new ShipmentService(uow, _cargoValidator, _webhookPublisher);

        // Act
        var result = await service.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_NonExisting_ReturnsFalse()
    {
        // Arrange
        var (uow, ctx) = _fixture.CreateUnitOfWork();
        var service = new ShipmentService(uow, _cargoValidator, _webhookPublisher);

        // Act
        var exists = await service.ExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(exists);
    }
}
