using Parhelion.Domain.Entities;
using Parhelion.Application.Auth;
using Parhelion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Parhelion.API.Data;

/// <summary>
/// Seed inicial del sistema con SuperUser y DefaultTenant.
/// Se ejecuta solo si no existen datos en la base de datos.
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Aplica el seeder si la base de datos está vacía.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ParhelionDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        // Skip si ya hay datos
        if (await context.Tenants.AnyAsync())
        {
            return;
        }
        
        // Datos del SuperUser desde config o defaults seguros
        var superEmail = config["Seed:SuperUserEmail"] ?? "metacodex@parhelion.com";
        var superPassword = config["Seed:SuperUserPassword"];
        
        if (string.IsNullOrEmpty(superPassword))
        {
            Console.WriteLine("⚠️  SEED: No SuperUserPassword configured. Skipping seeder.");
            Console.WriteLine("    Set Seed:SuperUserPassword in .env or appsettings to enable seeding.");
            return;
        }

        Console.WriteLine("🌱 Seeding database with initial data...");
        
        // 1. Crear DefaultTenant
        var defaultTenant = new Tenant
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            CompanyName = "Parhelion Logistics",
            ContactEmail = "admin@parhelion.com",
            FleetSize = 0,
            DriverCount = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Tenants.Add(defaultTenant);
        
        // 2. Crear Role SuperAdmin
        var superAdminRole = new Role
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "SuperAdmin",
            Description = "Super Administrator with full system access",
            CreatedAt = DateTime.UtcNow
        };
        context.Roles.Add(superAdminRole);
        
        // 3. Crear SuperUser con password hasheado (work factor 14 para admin)
        var superUser = new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            TenantId = defaultTenant.Id,
            Email = superEmail,
            PasswordHash = passwordHasher.HashPassword(superPassword, useArgon2: true),
            FullName = "MetaCodeX SuperAdmin",
            RoleId = superAdminRole.Id,
            IsActive = true,
            UsesArgon2 = true, // Indica que usa el hash más fuerte
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(superUser);
        
        await context.SaveChangesAsync();
        
        Console.WriteLine("✅ SEED: SuperUser created successfully");
        Console.WriteLine($"   Email: {superEmail}");
        Console.WriteLine($"   Role: SuperAdmin");
        Console.WriteLine($"   Tenant: Parhelion Logistics");
    }
}
