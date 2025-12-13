using Microsoft.EntityFrameworkCore;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Data;

namespace Parhelion.Tests;

/// <summary>
/// Tests de integración E2E para el Employee Layer (v0.4.3).
/// Verifica el flujo completo: Tenant → User → Employee → Driver/WarehouseOperator.
/// </summary>
public class EmployeeLayerIntegrationTests : IDisposable
{
    private readonly ParhelionDbContext _context;
    private readonly Guid _tenantId = Guid.NewGuid();

    public EmployeeLayerIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ParhelionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ParhelionDbContext(options, _tenantId);
        
        // Seed de roles
        SeedRoles();
    }

    private void SeedRoles()
    {
        _context.Roles.AddRange(
            new Role { Id = SeedData.AdminRoleId, Name = "Admin" },
            new Role { Id = SeedData.DriverRoleId, Name = "Driver" },
            new Role { Id = SeedData.WarehouseRoleId, Name = "Warehouse" },
            new Role { Id = SeedData.DemoUserRoleId, Name = "DemoUser" },
            new Role { Id = SeedData.SystemAdminRoleId, Name = "SystemAdmin" }
        );
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ========== TEST 1: Crear Tenant ==========
    
    [Fact]
    public async Task CanCreateTenant()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = _tenantId,
            CompanyName = "Test Company",
            ContactEmail = "admin@test.parhelion.com",
            FleetSize = 10,
            DriverCount = 5,
            IsActive = true
        };

        // Act
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        // Assert
        var savedTenant = await _context.Tenants.FindAsync(_tenantId);
        Assert.NotNull(savedTenant);
        Assert.Equal("Test Company", savedTenant.CompanyName);
    }

    // ========== TEST 2: Crear User con IsSuperAdmin ==========
    
    [Fact]
    public async Task CanCreateSuperAdminUser()
    {
        // Arrange - Crear tenant primero
        var tenant = new Tenant
        {
            Id = _tenantId,
            CompanyName = "Test Company",
            ContactEmail = "admin@test.parhelion.com",
            IsActive = true
        };
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        // Act - Crear super admin
        var superAdmin = new User
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId, // Para test, en producción sería null
            Email = "superadmin@parhelion.com",
            PasswordHash = "hashed_password",
            FullName = "Super Admin",
            RoleId = SeedData.SystemAdminRoleId,
            IsSuperAdmin = true,
            IsActive = true
        };

        _context.Users.Add(superAdmin);
        await _context.SaveChangesAsync();

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.IsSuperAdmin);
        Assert.NotNull(savedUser);
        Assert.True(savedUser.IsSuperAdmin);
        Assert.Equal("superadmin@parhelion.com", savedUser.Email);
    }

    // ========== TEST 3: Flujo completo User → Employee → Driver ==========
    
    [Fact]
    public async Task CanCreateCompleteDriverFlow()
    {
        // Arrange - Crear tenant
        var tenant = new Tenant
        {
            Id = _tenantId,
            CompanyName = "Transportes Norte",
            ContactEmail = "admin@norte.parhelion.com",
            FleetSize = 5,
            IsActive = true
        };
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        // Act 1 - Crear User (cuenta de acceso)
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            TenantId = _tenantId,
            Email = "carlos@norte.parhelion.com",
            PasswordHash = "hashed",
            FullName = "Carlos Pérez",
            RoleId = SeedData.DriverRoleId,
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act 2 - Crear Employee (datos laborales)
        var employeeId = Guid.NewGuid();
        var employee = new Employee
        {
            Id = employeeId,
            TenantId = _tenantId,
            UserId = userId,
            Phone = "8181234567",
            Rfc = "PERC901231ABC",
            Nss = "12345678901",
            Curp = "PERC901231HNLRRL09",
            EmergencyContact = "María Pérez",
            EmergencyPhone = "8187654321",
            HireDate = DateTime.UtcNow.AddMonths(-6),
            Department = "Field"
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Act 3 - Crear Driver (extensión con licencia)
        var driver = new Driver
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            LicenseNumber = "NL-2024-123456",
            LicenseType = "E",
            LicenseExpiration = DateTime.UtcNow.AddYears(2),
            Status = DriverStatus.Available
        };
        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync();

        // Assert - Verificar navegación completa
        var savedDriver = await _context.Drivers
            .Include(d => d.Employee)
                .ThenInclude(e => e.User)
            .FirstOrDefaultAsync();

        Assert.NotNull(savedDriver);
        Assert.NotNull(savedDriver.Employee);
        Assert.NotNull(savedDriver.Employee.User);
        
        // Verificar datos
        Assert.Equal("NL-2024-123456", savedDriver.LicenseNumber);
        Assert.Equal("PERC901231ABC", savedDriver.Employee.Rfc);
        Assert.Equal("carlos@norte.parhelion.com", savedDriver.Employee.User.Email);
    }

    // ========== TEST 4: Flujo completo User → Employee → WarehouseOperator ==========
    
    [Fact]
    public async Task CanCreateCompleteWarehouseOperatorFlow()
    {
        // Arrange - Crear tenant y ubicación
        var tenant = new Tenant
        {
            Id = _tenantId,
            CompanyName = "Transportes Norte",
            ContactEmail = "admin@norte.parhelion.com",
            IsActive = true
        };
        _context.Tenants.Add(tenant);

        var locationId = Guid.NewGuid();
        var location = new Location
        {
            Id = locationId,
            TenantId = _tenantId,
            Code = "MTY",
            Name = "CEDIS Monterrey",
            Type = LocationType.Warehouse,
            FullAddress = "Av. Industrial 123",
            CanReceive = true,
            CanDispatch = true,
            IsInternal = true,
            IsActive = true
        };
        _context.Locations.Add(location);

        // Crear zona de bodega
        var zoneId = Guid.NewGuid();
        var zone = new WarehouseZone
        {
            Id = zoneId,
            LocationId = locationId,
            Code = "A1",
            Name = "Zona de Recepción",
            Type = WarehouseZoneType.Receiving,
            IsActive = true
        };
        _context.WarehouseZones.Add(zone);
        await _context.SaveChangesAsync();

        // Act 1 - Crear User
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            TenantId = _tenantId,
            Email = "maria@norte.parhelion.com",
            PasswordHash = "hashed",
            FullName = "María García",
            RoleId = SeedData.WarehouseRoleId,
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act 2 - Crear Employee
        var employeeId = Guid.NewGuid();
        var employee = new Employee
        {
            Id = employeeId,
            TenantId = _tenantId,
            UserId = userId,
            Phone = "8189876543",
            Rfc = "GARM850515XYZ",
            Department = "Operations"
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Act 3 - Crear WarehouseOperator
        var warehouseOperator = new WarehouseOperator
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            AssignedLocationId = locationId,
            PrimaryZoneId = zoneId
        };
        _context.WarehouseOperators.Add(warehouseOperator);
        await _context.SaveChangesAsync();

        // Assert
        var savedOperator = await _context.WarehouseOperators
            .Include(w => w.Employee)
                .ThenInclude(e => e.User)
            .Include(w => w.AssignedLocation)
            .Include(w => w.PrimaryZone)
            .FirstOrDefaultAsync();

        Assert.NotNull(savedOperator);
        Assert.NotNull(savedOperator.Employee);
        Assert.NotNull(savedOperator.AssignedLocation);
        Assert.NotNull(savedOperator.PrimaryZone);
        
        Assert.Equal("maria@norte.parhelion.com", savedOperator.Employee.User.Email);
        Assert.Equal("MTY", savedOperator.AssignedLocation.Code);
        Assert.Equal("A1", savedOperator.PrimaryZone.Code);
    }

    // ========== TEST 5: Crear Shift y asignar a Employee ==========
    
    [Fact]
    public async Task CanCreateShiftAndAssignToEmployee()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = _tenantId,
            CompanyName = "Test Company",
            ContactEmail = "admin@test.parhelion.com",
            IsActive = true
        };
        _context.Tenants.Add(tenant);

        // Crear turno
        var shiftId = Guid.NewGuid();
        var shift = new Shift
        {
            Id = shiftId,
            TenantId = _tenantId,
            Name = "Turno Matutino",
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            DaysOfWeek = "Mon,Tue,Wed,Thu,Fri",
            IsActive = true
        };
        _context.Shifts.Add(shift);
        await _context.SaveChangesAsync();

        // Crear User y Employee con turno
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            TenantId = _tenantId,
            Email = "test@test.parhelion.com",
            PasswordHash = "hashed",
            FullName = "Test User",
            RoleId = SeedData.AdminRoleId,
            IsActive = true
        };
        _context.Users.Add(user);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            UserId = userId,
            Phone = "1234567890",
            ShiftId = shiftId,
            Department = "Admin"
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Assert
        var savedEmployee = await _context.Employees
            .Include(e => e.Shift)
            .FirstOrDefaultAsync();

        Assert.NotNull(savedEmployee);
        Assert.NotNull(savedEmployee.Shift);
        Assert.Equal("Turno Matutino", savedEmployee.Shift.Name);
        Assert.Equal(new TimeOnly(6, 0), savedEmployee.Shift.StartTime);
    }

    // ========== TEST 6: Verificar nuevos permisos existen ==========
    
    [Fact]
    public void NewPermissionsExist()
    {
        // Assert - Verificar que los nuevos permisos están definidos
        Assert.True(Enum.IsDefined(typeof(Permission), Permission.EmployeesRead));
        Assert.True(Enum.IsDefined(typeof(Permission), Permission.ShiftsRead));
        Assert.True(Enum.IsDefined(typeof(Permission), Permission.WarehouseZonesRead));
        Assert.True(Enum.IsDefined(typeof(Permission), Permission.WarehouseOperatorsRead));
        Assert.True(Enum.IsDefined(typeof(Permission), Permission.TenantsRead));
        Assert.True(Enum.IsDefined(typeof(Permission), Permission.TenantsCreate));
    }

    // ========== TEST 7: ShipmentCheckpoint con WarehouseOperator ==========
    
    [Fact]
    public async Task CanCreateCheckpointWithWarehouseOperator()
    {
        // Arrange - Setup completo
        var tenant = new Tenant
        {
            Id = _tenantId,
            CompanyName = "Test",
            ContactEmail = "test@test.com",
            IsActive = true
        };
        _context.Tenants.Add(tenant);

        var locationId = Guid.NewGuid();
        var location = new Location
        {
            Id = locationId,
            TenantId = _tenantId,
            Code = "WH1",
            Name = "Warehouse 1",
            Type = LocationType.Warehouse,
            FullAddress = "Test Address",
            IsActive = true
        };
        _context.Locations.Add(location);

        // Crear shipment
        var shipmentId = Guid.NewGuid();
        var shipment = new Shipment
        {
            Id = shipmentId,
            TenantId = _tenantId,
            TrackingNumber = "PAR-TEST01",
            QrCodeData = "QR-TEST01",
            OriginLocationId = locationId,
            DestinationLocationId = locationId,
            RecipientName = "Test Recipient",
            TotalWeightKg = 100,
            TotalVolumeM3 = 1,
            Status = ShipmentStatus.PendingApproval,
            Priority = ShipmentPriority.Normal
        };
        _context.Shipments.Add(shipment);

        // Crear User/Employee/WarehouseOperator
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            TenantId = _tenantId,
            Email = "wh@test.com",
            PasswordHash = "hash",
            FullName = "WH User",
            RoleId = SeedData.WarehouseRoleId,
            IsActive = true
        };
        _context.Users.Add(user);

        var employeeId = Guid.NewGuid();
        var employee = new Employee
        {
            Id = employeeId,
            TenantId = _tenantId,
            UserId = userId,
            Phone = "123",
            Department = "Operations"
        };
        _context.Employees.Add(employee);

        var warehouseOpId = Guid.NewGuid();
        var warehouseOp = new WarehouseOperator
        {
            Id = warehouseOpId,
            EmployeeId = employeeId,
            AssignedLocationId = locationId
        };
        _context.WarehouseOperators.Add(warehouseOp);
        await _context.SaveChangesAsync();

        // Act - Crear checkpoint con referencia a WarehouseOperator
        var checkpoint = new ShipmentCheckpoint
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipmentId,
            LocationId = locationId,
            StatusCode = CheckpointStatus.ArrivedHub,
            Timestamp = DateTime.UtcNow,
            CreatedByUserId = userId,
            HandledByWarehouseOperatorId = warehouseOpId,
            ActionType = "Received"
        };
        _context.ShipmentCheckpoints.Add(checkpoint);
        await _context.SaveChangesAsync();

        // Assert
        var savedCheckpoint = await _context.ShipmentCheckpoints
            .Include(c => c.HandledByWarehouseOperator)
                .ThenInclude(w => w!.Employee)
            .FirstOrDefaultAsync();

        Assert.NotNull(savedCheckpoint);
        Assert.NotNull(savedCheckpoint.HandledByWarehouseOperatorId);
        Assert.Equal(warehouseOpId, savedCheckpoint.HandledByWarehouseOperatorId);
    }
}
