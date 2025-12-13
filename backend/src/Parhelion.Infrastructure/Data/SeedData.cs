using Microsoft.EntityFrameworkCore;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Data;

/// <summary>
/// Seed Data para inicialización de la base de datos.
/// Incluye roles del sistema y opcionalmente un tenant demo.
/// </summary>
public static class SeedData
{
    // IDs fijos para roles del sistema (nunca cambian)
    public static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DriverRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid DemoUserRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid WarehouseRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    /// <summary>
    /// Inicializa la base de datos con seed data.
    /// Es seguro llamar múltiples veces (idempotente).
    /// </summary>
    public static async Task InitializeAsync(ParhelionDbContext context)
    {
        await SeedRolesAsync(context);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seed de roles del sistema con IDs fijos.
    /// Admin, Driver, Warehouse, DemoUser.
    /// </summary>
    private static async Task SeedRolesAsync(ParhelionDbContext context)
    {
        var roles = new[]
        {
            new Role
            {
                Id = AdminRoleId,
                Name = "Admin",
                Description = "Gerente de Tráfico - Acceso total al sistema"
            },
            new Role
            {
                Id = DriverRoleId,
                Name = "Driver",
                Description = "Chofer - Solo ve sus envíos asignados"
            },
            new Role
            {
                Id = WarehouseRoleId,
                Name = "Warehouse",
                Description = "Almacenista - Gestiona carga y descarga de camiones"
            },
            new Role
            {
                Id = DemoUserRoleId,
                Name = "DemoUser",
                Description = "Usuario de demostración temporal (24-48h)"
            }
        };

        foreach (var role in roles)
        {
            var existingRole = await context.Roles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == role.Id);
                
            if (existingRole == null)
            {
                context.Roles.Add(role);
            }
        }
    }

    /// <summary>
    /// Crea un tenant demo con datos de prueba.
    /// Usado para la Demo Pública del portafolio.
    /// </summary>
    public static async Task<Tenant> CreateDemoTenantAsync(ParhelionDbContext context, string companyName = "Demo Company")
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            CompanyName = companyName,
            ContactEmail = "demo@parhelion.lat",
            FleetSize = 5,
            DriverCount = 3,
            IsActive = true
        };

        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        // Seed ubicaciones demo
        await SeedDemoLocationsAsync(context, tenant.Id);
        
        // Seed camiones demo
        await SeedDemoTrucksAsync(context, tenant.Id);

        return tenant;
    }

    private static async Task SeedDemoLocationsAsync(ParhelionDbContext context, Guid tenantId)
    {
        var locations = new[]
        {
            new Location
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "MTY",
                Name = "CEDIS Monterrey",
                Type = LocationType.RegionalHub,
                FullAddress = "Av. Eugenio Garza Sada 2501, Tecnológico, Monterrey, N.L.",
                CanReceive = true,
                CanDispatch = true,
                IsInternal = true,
                IsActive = true
            },
            new Location
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "GDL",
                Name = "Hub Guadalajara",
                Type = LocationType.CrossDock,
                FullAddress = "Av. Patria 1501, Zapopan, Jalisco",
                CanReceive = true,
                CanDispatch = true,
                IsInternal = true,
                IsActive = true
            },
            new Location
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "CDMX",
                Name = "Almacén Ciudad de México",
                Type = LocationType.Warehouse,
                FullAddress = "Calle Río Churubusco 350, Iztapalapa, CDMX",
                CanReceive = true,
                CanDispatch = true,
                IsInternal = true,
                IsActive = true
            }
        };

        context.Locations.AddRange(locations);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDemoTrucksAsync(ParhelionDbContext context, Guid tenantId)
    {
        var trucks = new[]
        {
            new Truck
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Plate = "NL-001-X",
                Model = "Kenworth T680",
                Type = TruckType.DryBox,
                MaxCapacityKg = 25000,
                MaxVolumeM3 = 80,
                IsActive = true
            },
            new Truck
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Plate = "NL-002-R",
                Model = "Freightliner Cascadia",
                Type = TruckType.Refrigerated,
                MaxCapacityKg = 20000,
                MaxVolumeM3 = 60,
                IsActive = true
            },
            new Truck
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Plate = "NL-003-H",
                Model = "Peterbilt 579",
                Type = TruckType.HazmatTank,
                MaxCapacityKg = 30000,
                MaxVolumeM3 = 40,
                IsActive = true
            }
        };

        context.Trucks.AddRange(trucks);
        await context.SaveChangesAsync();
    }
}
