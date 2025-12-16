using Microsoft.EntityFrameworkCore;
using Parhelion.Domain.Entities;
using Parhelion.Infrastructure.Data;

namespace Parhelion.Tests.Fixtures;

/// <summary>
/// Fixture para tests con base de datos en memoria.
/// Proporciona un DbContext limpio para cada test.
/// </summary>
public class InMemoryDbFixture : IDisposable
{
    /// <summary>
    /// Crea un nuevo DbContext para el test.
    /// Cada llamada crea una base de datos única.
    /// </summary>
    public ParhelionDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ParhelionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
            
        var context = new ParhelionDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Crea un DbContext con datos de prueba sembrados.
    /// </summary>
    public ParhelionDbContext CreateSeededContext()
    {
        var context = CreateContext();
        SeedTestData(context);
        return context;
    }

    private void SeedTestData(ParhelionDbContext context)
    {
        // Roles seed
        var adminRole = new Role 
        { 
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), 
            Name = "Admin", 
            Description = "Administrador",
            CreatedAt = DateTime.UtcNow
        };
        var driverRole = new Role 
        { 
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), 
            Name = "Driver", 
            Description = "Chofer",
            CreatedAt = DateTime.UtcNow
        };
        var warehouseRole = new Role 
        { 
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), 
            Name = "Warehouse", 
            Description = "Almacenista",
            CreatedAt = DateTime.UtcNow
        };
        
        context.Roles.AddRange(adminRole, driverRole, warehouseRole);

        // Test Tenant
        var tenant = new Tenant
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            CompanyName = "Test Company",
            ContactEmail = "test@company.com",
            FleetSize = 5,
            DriverCount = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Tenants.Add(tenant);

        // Test User (Admin)
        var adminUser = new User
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            TenantId = tenant.Id,
            Email = "admin@test.com",
            PasswordHash = "hashedpassword",
            FullName = "Test Admin",
            RoleId = adminRole.Id,
            IsActive = true,
            IsDemoUser = false,
            IsSuperAdmin = false,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(adminUser);

        context.SaveChanges();
    }

    public void Dispose()
    {
        // No cleanup needed for in-memory database
    }
}

/// <summary>
/// Builder para crear datos de prueba.
/// </summary>
public static class TestDataBuilder
{
    public static Tenant CreateTenant(string name = "Test Co", bool isActive = true)
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            CompanyName = name,
            ContactEmail = $"{name.ToLower().Replace(" ", "")}@test.com",
            FleetSize = 5,
            DriverCount = 3,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User CreateUser(Guid tenantId, Guid roleId, string email = "user@test.com")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email,
            PasswordHash = "hashedpassword",
            FullName = "Test User",
            RoleId = roleId,
            IsActive = true,
            IsDemoUser = false,
            IsSuperAdmin = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Truck CreateTruck(Guid tenantId, string plate = "TEST-001")
    {
        return new Truck
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Plate = plate,
            Model = "Test Model",
            Type = Domain.Enums.TruckType.DryBox,
            MaxCapacityKg = 10000,
            MaxVolumeM3 = 50,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Location CreateLocation(Guid tenantId, string code = "TST")
    {
        return new Location
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = code,
            Name = $"Test Location {code}",
            Type = Domain.Enums.LocationType.RegionalHub,
            FullAddress = "123 Test Street",
            CanReceive = true,
            CanDispatch = true,
            IsInternal = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Shipment CreateShipment(
        Guid tenantId, 
        Guid originId, 
        Guid destinationId,
        string trackingNumber = "PAR-TEST01")
    {
        return new Shipment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrackingNumber = trackingNumber,
            QrCodeData = Guid.NewGuid().ToString(),
            OriginLocationId = originId,
            DestinationLocationId = destinationId,
            RecipientName = "Test Recipient",
            RecipientPhone = "555-0100",
            TotalWeightKg = 100,
            TotalVolumeM3 = 1,
            Priority = Domain.Enums.ShipmentPriority.Normal,
            Status = Domain.Enums.ShipmentStatus.PendingApproval,
            CreatedAt = DateTime.UtcNow
        };
    }
}
