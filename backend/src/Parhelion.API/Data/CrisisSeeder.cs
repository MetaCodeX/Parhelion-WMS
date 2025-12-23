using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Application.Auth;
using Parhelion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Parhelion.API.Data;

/// <summary>
/// Seeder específico para el flujo de Crisis Management.
/// Crea los 3 escenarios de prueba: Victima, Rescate, Lejano.
/// </summary>
public static class CrisisSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ParhelionDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // 1. Obtener Tenant principal (asumimos que DataSeeder ya corrió)
        var tenant = await context.Tenants.FirstOrDefaultAsync();
        if (tenant == null)
        {
            Console.WriteLine("⚠️ CRISIS SEED: No tenant found. Skipping.");
            return;
        }

        // 2. Verificar si ya existen los datos para no duplicar
        if (await context.Trucks.AnyAsync(t => t.Plate == "VICTIM-01"))
        {
            Console.WriteLine("ℹ️ CRISIS SEED: Data already exists. Skipping.");
            return;
        }

        Console.WriteLine("🚑 CRISIS SEED: Injecting test scenario data...");

        // 3. Obtener Rol de Driver (asumimos ID fijo o buscamos por nombre)
        var driverRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Driver");
        if (driverRole == null)
        {
             // Si no existe, usamos cualquier rol o lanzamos error. Por ahora creamos uno dummy en memoria si falla.
             // Pero en producción/dev ya debería existir por DataSeeder o migraciones anteriores.
             // Buscaremos el ID fijo de la documentación si falla el nombre
             driverRole = await context.Roles.FindAsync(Guid.Parse("22222222-2222-2222-2222-222222222222"));
             if (driverRole == null) throw new Exception("Driver role not found for seeding");
        }

        var defaultPass = passwordHasher.HashPassword("Test1234!");

        // --- SCENARIO 1: VICTIM (El que se rompe) ---
        // Coordenadas: 20.588056, -100.388056
        CreateDriverStack(context, tenant.Id, driverRole.Id, defaultPass, 
            firstName: "Victim", 
            lastName: "Driver", 
            email: "victim@parhelion.com", 
            plate: "VICTIM-01", 
            lat: 20.588056m, lon: -100.388056m,
            status: DriverStatus.OnRoute, // Está ocupado/en ruta antes de fallar
            truckType: TruckType.DryBox);

        // --- SCENARIO 2: RESCUE (El salvador cercano) ---
        // Coordenadas: 20.612000, -100.410000 (~3-4 km cerca)
        CreateDriverStack(context, tenant.Id, driverRole.Id, defaultPass, 
            firstName: "Rescue", 
            lastName: "Driver", 
            email: "rescue@parhelion.com", 
            plate: "RESCUE-01", 
            lat: 20.612000m, lon: -100.410000m, 
            status: DriverStatus.Available, // Debe estar disponible
            truckType: TruckType.DryBox);

        // --- SCENARIO 3: FAR (El que está en CDMX, lejos) ---
        // Coordenadas: 19.432608, -99.133209 (~200 km lejos)
        CreateDriverStack(context, tenant.Id, driverRole.Id, defaultPass, 
            firstName: "Far", 
            lastName: "Driver", 
            email: "far@parhelion.com", 
            plate: "FAR-01", 
            lat: 19.432608m, lon: -99.133209m, 
            status: DriverStatus.Available, // Disponible pero lejos
            truckType: TruckType.DryBox);

        await context.SaveChangesAsync();
        Console.WriteLine("✅ CRISIS SEED: Scenario data injected successfully.");
    }

    private static void CreateDriverStack(
        ParhelionDbContext ctx, 
        Guid tenantId, 
        Guid roleId, 
        string passwordHash,
        string firstName, 
        string lastName, 
        string email, 
        string plate, 
        decimal lat, 
        decimal lon,
        DriverStatus status,
        TruckType truckType)
    {
        // 1. User
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email,
            PasswordHash = passwordHash,
            FullName = $"{firstName} {lastName}",
            RoleId = roleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UsesArgon2 = true // Asumimos default moderno
        };
        ctx.Users.Add(user);

        // 2. Employee
        var emp = new Employee
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = user.Id,
            User = user,
            Phone = "555-000-0000",
            Department = "Fleet",
            HireDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        ctx.Employees.Add(emp);

        // 3. Truck
        var truck = new Truck
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Plate = plate,
            Model = "Generic Test Truck",
            Type = truckType,
            MaxCapacityKg = 15000,
            MaxVolumeM3 = 60,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLatitude = lat,
            LastLongitude = lon,
            LastLocationUpdate = DateTime.UtcNow
        };
        ctx.Trucks.Add(truck);

        // 4. Driver
        var driver = new Driver
        {
            Id = Guid.NewGuid(),
            EmployeeId = emp.Id,
            Employee = emp,
            LicenseNumber = $"LIC-{plate}",
            Status = status,
            CurrentTruckId = truck.Id,
            CurrentTruck = truck,
            // TenantId eliminado, no existe en Driver
            CreatedAt = DateTime.UtcNow
        };
        ctx.Drivers.Add(driver);
    }
}
