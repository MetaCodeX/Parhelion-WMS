using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Infrastructure.Services.Fleet;
using Parhelion.Tests.Fixtures;
using Xunit;

namespace Parhelion.Tests.Integration;

/// <summary>
/// Tests de integración para el área de Fleet Management (TMS - Fleet).
/// Verifica flujos completos que involucran Trucks, Drivers, FleetLogs.
/// </summary>
public class FleetIntegrationTests : IClassFixture<ServiceTestFixture>
{
    private readonly ServiceTestFixture _fixture;

    public FleetIntegrationTests(ServiceTestFixture fixture) => _fixture = fixture;

    /// <summary>
    /// Flujo completo: Crear camión → Crear driver → Asignar → Registrar log
    /// </summary>
    [Fact]
    public async Task FleetFlow_CreateTruckAndDriver_AssignAndLog()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var truckService = new TruckService(uow);
        var driverService = new DriverService(uow);
        var logService = new FleetLogService(uow);

        // Act 1: Create truck
        var truckResult = await truckService.CreateAsync(new CreateTruckRequest(
            Plate: "FLEET-001",
            Model: "Volvo FH16",
            Type: "Refrigerated",
            MaxCapacityKg: 25000,
            MaxVolumeM3: 80,
            Vin: "VIN123456789", EngineNumber: null, Year: 2024, Color: "White",
            InsurancePolicy: "INS-001", InsuranceExpiration: DateTime.UtcNow.AddYears(1),
            VerificationNumber: null, VerificationExpiration: null));
        Assert.True(truckResult.Success);
        var truckId = truckResult.Data!.Id;

        // Act 2: Create driver
        var driverResult = await driverService.CreateAsync(new CreateDriverRequest(
            EmployeeId: ids.EmployeeId,
            LicenseNumber: "LIC-FLEET-001",
            LicenseType: "Federal",
            LicenseExpiration: DateTime.UtcNow.AddYears(2),
            DefaultTruckId: null,
            Status: "Available"));
        Assert.True(driverResult.Success);
        var driverId = driverResult.Data!.Id;

        // Act 3: Assign driver to truck
        var assignResult = await driverService.AssignTruckAsync(driverId, truckId);
        Assert.True(assignResult.Success);
        Assert.Equal(truckId, assignResult.Data!.CurrentTruckId);

        // Act 4: Create fleet log for assignment via StartUsageAsync
        var logResult = await logService.StartUsageAsync(driverId, truckId);
        Assert.True(logResult.Success);
    }

    /// <summary>
    /// Flujo: Verificar que un driver puede cambiar de camión
    /// </summary>
    [Fact]
    public async Task FleetGuard_DriverCanChangeTruck()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var truckService = new TruckService(uow);
        var driverService = new DriverService(uow);

        // Create driver
        var driverResult = await driverService.CreateAsync(new CreateDriverRequest(
            EmployeeId: ids.EmployeeId,
            LicenseNumber: "LIC-GUARD-001",
            LicenseType: "CDL-A",
            LicenseExpiration: DateTime.UtcNow.AddYears(2),
            DefaultTruckId: null,
            Status: "Available"));
        var driverId = driverResult.Data!.Id;

        // Create two trucks
        var truck1 = await truckService.CreateAsync(new CreateTruckRequest("GUARD-01", "Truck1", "DryBox", 10000, 40, null, null, null, null, null, null, null, null));
        var truck2 = await truckService.CreateAsync(new CreateTruckRequest("GUARD-02", "Truck2", "DryBox", 10000, 40, null, null, null, null, null, null, null, null));

        // Act: Assign to first truck
        var assign1 = await driverService.AssignTruckAsync(driverId, truck1.Data!.Id);
        Assert.True(assign1.Success);

        // Act: Assign to second truck (should reassign)
        var assign2 = await driverService.AssignTruckAsync(driverId, truck2.Data!.Id);
        
        // Assert: Should have changed to new truck
        Assert.True(assign2.Success);
        Assert.Equal(truck2.Data!.Id, assign2.Data!.CurrentTruckId);
    }

    /// <summary>
    /// Flujo: Desactivar camión y verificar que aparece en queries filtradas
    /// </summary>
    [Fact]
    public async Task FleetManagement_TruckActiveStatus()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var truckService = new TruckService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 20 };

        // Act: Set truck to inactive
        await truckService.SetActiveStatusAsync(ids.TruckId, false);
        var activeTrucks = await truckService.GetByActiveStatusAsync(ids.TenantId, true, request);

        // Assert: Inactive truck should not be in active list
        Assert.True(activeTrucks.Items.All(t => t.IsActive));
    }

    /// <summary>
    /// Flujo: Buscar logs por camión específico
    /// </summary>
    [Fact]
    public async Task FleetLogs_TrackByTruck()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var driverService = new DriverService(uow);
        var logService = new FleetLogService(uow);

        // Create a driver first
        var driverResult = await driverService.CreateAsync(new CreateDriverRequest(
            ids.EmployeeId, "LIC-LOG-001", "Federal", DateTime.UtcNow.AddYears(2), null, "Available"));
        var driverId = driverResult.Data!.Id;

        // Create fleet logs via StartUsageAsync
        await logService.StartUsageAsync(driverId, ids.TruckId);

        // Act: Get logs for truck
        var request = new PagedRequest { Page = 1, PageSize = 20 };
        var truckLogs = await logService.GetByTruckAsync(ids.TruckId, request);

        // Assert: Should have at least 1 log
        Assert.True(truckLogs.TotalCount >= 1);
    }
}
