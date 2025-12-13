using Microsoft.EntityFrameworkCore;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Data;

namespace Parhelion.Tests;

/// <summary>
/// 🧪 SUPER TEST E2E - Flujo Completo del Sistema
/// 
/// Recorre TODAS las tablas del sistema en un flujo realista:
/// SuperAdmin → Tenant → Admin → Employees → Trucks → Drivers → Warehouse →
/// Locations → Zones → Routes → Clients → Shipments → Items → Checkpoints → Documents → Delivery
/// 
/// Este test valida que toda la base de datos está correctamente integrada.
/// </summary>
public class FullSystemE2ETest : IDisposable
{
    private readonly ParhelionDbContext _context;
    private readonly Guid _testTenantId = Guid.NewGuid();
    
    // IDs que se crean durante el test
    private Guid _superAdminUserId;
    private Guid _adminUserId;
    private Guid _adminEmployeeId;
    private Guid _driverUserId;
    private Guid _driverEmployeeId;
    private Guid _driverId;
    private Guid _warehouseUserId;
    private Guid _warehouseEmployeeId;
    private Guid _warehouseOperatorId;
    private Guid _truckId;
    private Guid _originLocationId;
    private Guid _hubLocationId;
    private Guid _destLocationId;
    private Guid _zoneId;
    private Guid _shiftId;
    private Guid _routeId;
    private Guid _senderId;
    private Guid _recipientId;
    private Guid _shipmentId;
    private Guid _shipmentItemId;
    private Guid _networkLinkId;

    public FullSystemE2ETest()
    {
        var options = new DbContextOptionsBuilder<ParhelionDbContext>()
            .UseInMemoryDatabase(databaseName: $"FullE2E_{Guid.NewGuid()}")
            .Options;

        _context = new ParhelionDbContext(options, _testTenantId);
        SeedSystemRoles();
    }

    private void SeedSystemRoles()
    {
        _context.Roles.AddRange(
            new Role { Id = SeedData.AdminRoleId, Name = "Admin", Description = "Gerente de Tráfico" },
            new Role { Id = SeedData.DriverRoleId, Name = "Driver", Description = "Chofer" },
            new Role { Id = SeedData.WarehouseRoleId, Name = "Warehouse", Description = "Almacenista" },
            new Role { Id = SeedData.DemoUserRoleId, Name = "DemoUser", Description = "Demo" },
            new Role { Id = SeedData.SystemAdminRoleId, Name = "SystemAdmin", Description = "Super Admin" }
        );
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    /// <summary>
    /// 🔥 TEST PRINCIPAL: Flujo completo desde SuperAdmin hasta entrega final
    /// </summary>
    [Fact]
    public async Task FullSystemFlow_SuperAdminToDelivery_AllTablesWork()
    {
        // ============================================================
        // FASE 1: SUPER ADMIN CREA TENANT Y ADMIN
        // ============================================================
        await Step1_SuperAdminCreatesTenantAndAdmin();
        
        // ============================================================
        // FASE 2: ADMIN CONFIGURA LA EMPRESA
        // ============================================================
        await Step2_AdminConfiguresShifts();
        await Step3_AdminCreatesLocationsAndZones();
        await Step4_AdminCreatesNetworkAndRoutes();
        await Step5_AdminCreatesTrucks();
        await Step6_AdminCreatesDriverEmployee();
        await Step7_AdminCreatesWarehouseOperator();
        await Step8_AdminCreatesClients();
        
        // ============================================================
        // FASE 3: OPERACIONES - CREACIÓN DE ENVÍO
        // ============================================================
        await Step9_CreateShipmentWithItems();
        
        // ============================================================
        // FASE 4: TRAZABILIDAD - CHECKPOINTS
        // ============================================================
        await Step10_WarehouseLoadsShipment();
        await Step11_DriverPicksUpShipment();
        await Step12_ShipmentArrivesAtHub();
        await Step13_ShipmentOutForDelivery();
        await Step14_ShipmentDelivered();
        
        // ============================================================
        // FASE 5: DOCUMENTACIÓN
        // ============================================================
        await Step15_GenerateDocuments();
        
        // ============================================================
        // FASE 6: VERIFICACIÓN FINAL
        // ============================================================
        await VerifyAllTablesHaveData();
    }

    // ==================== FASE 1: SUPER ADMIN ====================

    private async Task Step1_SuperAdminCreatesTenantAndAdmin()
    {
        // 1.1 Super Admin se registra (sin tenant)
        _superAdminUserId = Guid.NewGuid();
        var superAdmin = new User
        {
            Id = _superAdminUserId,
            TenantId = _testTenantId, // En test, requerido por InMemory
            Email = "superadmin@parhelion.com",
            PasswordHash = "hashed_super_secure",
            FullName = "Sistema Parhelion",
            RoleId = SeedData.SystemAdminRoleId,
            IsSuperAdmin = true,
            IsActive = true
        };
        _context.Users.Add(superAdmin);

        // 1.2 Crear Tenant (empresa)
        var tenant = new Tenant
        {
            Id = _testTenantId,
            CompanyName = "Transportes Norte S.A. de C.V.",
            ContactEmail = "admin@tnorte.parhelion.com",
            FleetSize = 10,
            DriverCount = 5,
            IsActive = true
        };
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        // 1.3 Super Admin crea Admin de la empresa
        _adminUserId = Guid.NewGuid();
        var adminUser = new User
        {
            Id = _adminUserId,
            TenantId = _testTenantId,
            Email = "juan.perez@tnorte.parhelion.com",
            PasswordHash = "hashed_admin",
            FullName = "Juan Pérez García",
            RoleId = SeedData.AdminRoleId,
            IsActive = true
        };
        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        // 1.4 Crear perfil de Employee para el Admin
        _adminEmployeeId = Guid.NewGuid();
        var adminEmployee = new Employee
        {
            Id = _adminEmployeeId,
            TenantId = _testTenantId,
            UserId = _adminUserId,
            Phone = "8181234567",
            Rfc = "PEGJ850101ABC",
            Nss = "12345678901",
            Curp = "PEGJ850101HNLRRL09",
            EmergencyContact = "María García",
            EmergencyPhone = "8187654321",
            HireDate = DateTime.UtcNow.AddYears(-5),
            Department = "Admin"
        };
        _context.Employees.Add(adminEmployee);
        await _context.SaveChangesAsync();

        // Verificación
        Assert.NotNull(await _context.Tenants.FindAsync(_testTenantId));
        Assert.NotNull(await _context.Users.FindAsync(_superAdminUserId));
        Assert.NotNull(await _context.Users.FindAsync(_adminUserId));
        Assert.NotNull(await _context.Employees.FindAsync(_adminEmployeeId));
    }

    // ==================== FASE 2: ADMIN CONFIGURA ====================

    private async Task Step2_AdminConfiguresShifts()
    {
        _shiftId = Guid.NewGuid();
        var shifts = new[]
        {
            new Shift
            {
                Id = _shiftId,
                TenantId = _testTenantId,
                Name = "Turno Matutino",
                StartTime = new TimeOnly(6, 0),
                EndTime = new TimeOnly(14, 0),
                DaysOfWeek = "Mon,Tue,Wed,Thu,Fri",
                IsActive = true
            },
            new Shift
            {
                Id = Guid.NewGuid(),
                TenantId = _testTenantId,
                Name = "Turno Vespertino",
                StartTime = new TimeOnly(14, 0),
                EndTime = new TimeOnly(22, 0),
                DaysOfWeek = "Mon,Tue,Wed,Thu,Fri",
                IsActive = true
            }
        };
        _context.Shifts.AddRange(shifts);
        await _context.SaveChangesAsync();

        Assert.Equal(2, await _context.Shifts.CountAsync());
    }

    private async Task Step3_AdminCreatesLocationsAndZones()
    {
        // 3.1 Ubicaciones
        _originLocationId = Guid.NewGuid();
        _hubLocationId = Guid.NewGuid();
        _destLocationId = Guid.NewGuid();

        var locations = new[]
        {
            new Location
            {
                Id = _originLocationId,
                TenantId = _testTenantId,
                Code = "MTY",
                Name = "CEDIS Monterrey",
                Type = LocationType.Warehouse,
                FullAddress = "Av. Eugenio Garza Sada 2501, Monterrey, NL",
                CanReceive = true,
                CanDispatch = true,
                IsInternal = true,
                IsActive = true
            },
            new Location
            {
                Id = _hubLocationId,
                TenantId = _testTenantId,
                Code = "SLP",
                Name = "Hub San Luis Potosí",
                Type = LocationType.CrossDock,
                FullAddress = "Carretera 57 Km 15, SLP",
                CanReceive = true,
                CanDispatch = true,
                IsInternal = true,
                IsActive = true
            },
            new Location
            {
                Id = _destLocationId,
                TenantId = _testTenantId,
                Code = "CDMX",
                Name = "Punto de Entrega CDMX",
                Type = LocationType.Store,
                FullAddress = "Calzada de Tlalpan 1234, CDMX",
                CanReceive = true,
                CanDispatch = false,
                IsInternal = false,
                IsActive = true
            }
        };
        _context.Locations.AddRange(locations);
        await _context.SaveChangesAsync();

        // 3.2 Zonas de bodega
        _zoneId = Guid.NewGuid();
        var zones = new[]
        {
            new WarehouseZone
            {
                Id = _zoneId,
                LocationId = _originLocationId,
                Code = "A1",
                Name = "Zona de Recepción",
                Type = WarehouseZoneType.Receiving,
                IsActive = true
            },
            new WarehouseZone
            {
                Id = Guid.NewGuid(),
                LocationId = _originLocationId,
                Code = "B1",
                Name = "Almacenamiento General",
                Type = WarehouseZoneType.Storage,
                IsActive = true
            },
            new WarehouseZone
            {
                Id = Guid.NewGuid(),
                LocationId = _originLocationId,
                Code = "C1",
                Name = "Andén de Salida",
                Type = WarehouseZoneType.Shipping,
                IsActive = true
            },
            new WarehouseZone
            {
                Id = Guid.NewGuid(),
                LocationId = _originLocationId,
                Code = "COLD-1",
                Name = "Cuarto Frío",
                Type = WarehouseZoneType.ColdChain,
                IsActive = true
            }
        };
        _context.WarehouseZones.AddRange(zones);
        await _context.SaveChangesAsync();

        Assert.Equal(3, await _context.Locations.CountAsync());
        Assert.Equal(4, await _context.WarehouseZones.CountAsync());
    }

    private async Task Step4_AdminCreatesNetworkAndRoutes()
    {
        // 4.1 Network Links (conexiones entre ubicaciones)
        _networkLinkId = Guid.NewGuid();
        var links = new[]
        {
            new NetworkLink
            {
                Id = _networkLinkId,
                TenantId = _testTenantId,
                OriginLocationId = _originLocationId,
                DestinationLocationId = _hubLocationId,
                LinkType = NetworkLinkType.LineHaul,
                TransitTime = TimeSpan.FromHours(5),
                IsBidirectional = true,
                IsActive = true
            },
            new NetworkLink
            {
                Id = Guid.NewGuid(),
                TenantId = _testTenantId,
                OriginLocationId = _hubLocationId,
                DestinationLocationId = _destLocationId,
                LinkType = NetworkLinkType.LastMile,
                TransitTime = TimeSpan.FromHours(4),
                IsBidirectional = false,
                IsActive = true
            }
        };
        _context.NetworkLinks.AddRange(links);
        await _context.SaveChangesAsync();

        // 4.2 Route Blueprint
        _routeId = Guid.NewGuid();
        var route = new RouteBlueprint
        {
            Id = _routeId,
            TenantId = _testTenantId,
            Name = "Ruta Norte-Centro",
            Description = "MTY → SLP → CDMX",
            TotalSteps = 3,
            TotalTransitTime = TimeSpan.FromHours(9),
            IsActive = true
        };
        _context.RouteBlueprints.Add(route);
        await _context.SaveChangesAsync();

        // 4.3 Route Steps
        var steps = new[]
        {
            new RouteStep
            {
                Id = Guid.NewGuid(),
                RouteBlueprintId = _routeId,
                LocationId = _originLocationId,
                StepOrder = 1,
                StandardTransitTime = TimeSpan.Zero,
                StepType = RouteStepType.Origin
            },
            new RouteStep
            {
                Id = Guid.NewGuid(),
                RouteBlueprintId = _routeId,
                LocationId = _hubLocationId,
                StepOrder = 2,
                StandardTransitTime = TimeSpan.FromHours(5),
                StepType = RouteStepType.Intermediate
            },
            new RouteStep
            {
                Id = Guid.NewGuid(),
                RouteBlueprintId = _routeId,
                LocationId = _destLocationId,
                StepOrder = 3,
                StandardTransitTime = TimeSpan.FromHours(4),
                StepType = RouteStepType.Destination
            }
        };
        _context.RouteSteps.AddRange(steps);
        await _context.SaveChangesAsync();

        Assert.Equal(2, await _context.NetworkLinks.CountAsync());
        Assert.Equal(1, await _context.RouteBlueprints.CountAsync());
        Assert.Equal(3, await _context.RouteSteps.CountAsync());
    }

    private async Task Step5_AdminCreatesTrucks()
    {
        _truckId = Guid.NewGuid();
        var trucks = new[]
        {
            new Truck
            {
                Id = _truckId,
                TenantId = _testTenantId,
                Plate = "NL-001-X",
                Model = "Kenworth T680",
                Type = TruckType.DryBox,
                MaxCapacityKg = 25000,
                MaxVolumeM3 = 80,
                Vin = "1XKYD49X8NJ123456",
                Year = 2023,
                InsurancePolicy = "POL-2024-001",
                InsuranceExpiration = DateTime.UtcNow.AddYears(1),
                IsActive = true
            },
            new Truck
            {
                Id = Guid.NewGuid(),
                TenantId = _testTenantId,
                Plate = "NL-002-R",
                Model = "Freightliner Cascadia",
                Type = TruckType.Refrigerated,
                MaxCapacityKg = 20000,
                MaxVolumeM3 = 60,
                IsActive = true
            }
        };
        _context.Trucks.AddRange(trucks);
        await _context.SaveChangesAsync();

        Assert.Equal(2, await _context.Trucks.CountAsync());
    }

    private async Task Step6_AdminCreatesDriverEmployee()
    {
        // 6.1 User para el chofer
        _driverUserId = Guid.NewGuid();
        var driverUser = new User
        {
            Id = _driverUserId,
            TenantId = _testTenantId,
            Email = "carlos.driver@tnorte.parhelion.com",
            PasswordHash = "hashed_driver",
            FullName = "Carlos Rodríguez",
            RoleId = SeedData.DriverRoleId,
            IsActive = true
        };
        _context.Users.Add(driverUser);
        await _context.SaveChangesAsync();

        // 6.2 Employee (datos laborales)
        _driverEmployeeId = Guid.NewGuid();
        var driverEmployee = new Employee
        {
            Id = _driverEmployeeId,
            TenantId = _testTenantId,
            UserId = _driverUserId,
            Phone = "8189876543",
            Rfc = "RODC900215XYZ",
            Nss = "98765432101",
            Curp = "RODC900215HNLRRL05",
            EmergencyContact = "Ana Rodríguez",
            EmergencyPhone = "8181112233",
            HireDate = DateTime.UtcNow.AddMonths(-18),
            ShiftId = _shiftId,
            Department = "Field"
        };
        _context.Employees.Add(driverEmployee);
        await _context.SaveChangesAsync();

        // 6.3 Driver (extensión con licencia)
        _driverId = Guid.NewGuid();
        var driver = new Driver
        {
            Id = _driverId,
            EmployeeId = _driverEmployeeId,
            LicenseNumber = "NL-2024-567890",
            LicenseType = "E",
            LicenseExpiration = DateTime.UtcNow.AddYears(3),
            DefaultTruckId = _truckId,
            CurrentTruckId = _truckId,
            Status = DriverStatus.Available
        };
        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync();

        // 6.4 Fleet Log (asignación inicial)
        var fleetLog = new FleetLog
        {
            Id = Guid.NewGuid(),
            TenantId = _testTenantId,
            DriverId = _driverId,
            OldTruckId = null,
            NewTruckId = _truckId,
            Reason = FleetLogReason.Reassignment,
            Timestamp = DateTime.UtcNow,
            CreatedByUserId = _adminUserId
        };
        _context.FleetLogs.Add(fleetLog);
        await _context.SaveChangesAsync();

        Assert.NotNull(await _context.Drivers.FindAsync(_driverId));
        Assert.Equal(1, await _context.FleetLogs.CountAsync());
    }

    private async Task Step7_AdminCreatesWarehouseOperator()
    {
        // 7.1 User para almacenista
        _warehouseUserId = Guid.NewGuid();
        var whUser = new User
        {
            Id = _warehouseUserId,
            TenantId = _testTenantId,
            Email = "maria.wh@tnorte.parhelion.com",
            PasswordHash = "hashed_wh",
            FullName = "María López",
            RoleId = SeedData.WarehouseRoleId,
            IsActive = true
        };
        _context.Users.Add(whUser);
        await _context.SaveChangesAsync();

        // 7.2 Employee
        _warehouseEmployeeId = Guid.NewGuid();
        var whEmployee = new Employee
        {
            Id = _warehouseEmployeeId,
            TenantId = _testTenantId,
            UserId = _warehouseUserId,
            Phone = "8185554433",
            Rfc = "LOPM880512ABC",
            ShiftId = _shiftId,
            Department = "Operations"
        };
        _context.Employees.Add(whEmployee);
        await _context.SaveChangesAsync();

        // 7.3 WarehouseOperator
        _warehouseOperatorId = Guid.NewGuid();
        var whOperator = new WarehouseOperator
        {
            Id = _warehouseOperatorId,
            EmployeeId = _warehouseEmployeeId,
            AssignedLocationId = _originLocationId,
            PrimaryZoneId = _zoneId
        };
        _context.WarehouseOperators.Add(whOperator);
        await _context.SaveChangesAsync();

        Assert.NotNull(await _context.WarehouseOperators.FindAsync(_warehouseOperatorId));
    }

    private async Task Step8_AdminCreatesClients()
    {
        // Remitente (quien envía)
        _senderId = Guid.NewGuid();
        var sender = new Client
        {
            Id = _senderId,
            TenantId = _testTenantId,
            CompanyName = "Fábrica de Electrónicos SA",
            ContactName = "Roberto Sánchez",
            Email = "roberto@fabricaelectronicos.mx",
            Phone = "5512345678",
            TaxId = "FEL901231XYZ",
            LegalName = "Fábrica de Electrónicos S.A. de C.V.",
            BillingAddress = "Parque Industrial Norte 123, Monterrey",
            ShippingAddress = "Parque Industrial Norte 123, Monterrey",
            Priority = ClientPriority.High,
            IsActive = true
        };
        _context.Clients.Add(sender);

        // Destinatario (quien recibe)
        _recipientId = Guid.NewGuid();
        var recipient = new Client
        {
            Id = _recipientId,
            TenantId = _testTenantId,
            CompanyName = "Tienda Electro CDMX",
            ContactName = "Laura Martínez",
            Email = "laura@electrocdmx.mx",
            Phone = "5598765432",
            ShippingAddress = "Calzada de Tlalpan 1234, CDMX",
            Priority = ClientPriority.Normal,
            IsActive = true
        };
        _context.Clients.Add(recipient);
        await _context.SaveChangesAsync();

        Assert.Equal(2, await _context.Clients.CountAsync());
    }

    // ==================== FASE 3: OPERACIONES ====================

    private async Task Step9_CreateShipmentWithItems()
    {
        // 9.1 Crear envío
        _shipmentId = Guid.NewGuid();
        var shipment = new Shipment
        {
            Id = _shipmentId,
            TenantId = _testTenantId,
            TrackingNumber = "PAR-E2E001",
            QrCodeData = "QR-PAR-E2E001",
            OriginLocationId = _originLocationId,
            DestinationLocationId = _destLocationId,
            SenderId = _senderId,
            RecipientClientId = _recipientId,
            RecipientName = "Laura Martínez",
            RecipientPhone = "5598765432",
            TotalWeightKg = 150,
            TotalVolumeM3 = 2.5m,
            DeclaredValue = 50000,
            SatMerchandiseCode = "84111506",
            DeliveryInstructions = "Entregar en horario de oficina",
            AssignedRouteId = _routeId,
            CurrentStepOrder = 1,
            Priority = ShipmentPriority.Normal,
            Status = ShipmentStatus.PendingApproval,
            ScheduledDeparture = DateTime.UtcNow.AddHours(2)
        };
        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync();

        // 9.2 Agregar items al envío
        _shipmentItemId = Guid.NewGuid();
        var items = new[]
        {
            new ShipmentItem
            {
                Id = _shipmentItemId,
                ShipmentId = _shipmentId,
                Sku = "ELEC-TV-55",
                Description = "Televisor LED 55 pulgadas",
                PackagingType = PackagingType.Box,
                Quantity = 5,
                WeightKg = 25,
                WidthCm = 130,
                HeightCm = 80,
                LengthCm = 20,
                DeclaredValue = 8000,
                IsFragile = true,
                IsHazardous = false,
                RequiresRefrigeration = false
            },
            new ShipmentItem
            {
                Id = Guid.NewGuid(),
                ShipmentId = _shipmentId,
                Sku = "ELEC-LAPTOP-15",
                Description = "Laptop Empresarial 15.6\"",
                PackagingType = PackagingType.Box,
                Quantity = 10,
                WeightKg = 2.5m,
                WidthCm = 40,
                HeightCm = 30,
                LengthCm = 10,
                DeclaredValue = 2500,
                IsFragile = true
            }
        };
        _context.ShipmentItems.AddRange(items);
        await _context.SaveChangesAsync();

        Assert.NotNull(await _context.Shipments.FindAsync(_shipmentId));
        Assert.Equal(2, await _context.ShipmentItems.CountAsync());
    }

    // ==================== FASE 4: TRAZABILIDAD ====================

    private async Task Step10_WarehouseLoadsShipment()
    {
        // Aprobar envío
        var shipment = await _context.Shipments.FindAsync(_shipmentId);
        shipment!.Status = ShipmentStatus.Approved;
        await _context.SaveChangesAsync();

        // Checkpoint: Almacenista carga el paquete
        var checkpoint = new ShipmentCheckpoint
        {
            Id = Guid.NewGuid(),
            ShipmentId = _shipmentId,
            LocationId = _originLocationId,
            StatusCode = CheckpointStatus.Loaded,
            Remarks = "Carga completada por almacenista",
            Timestamp = DateTime.UtcNow,
            CreatedByUserId = _warehouseUserId,
            HandledByWarehouseOperatorId = _warehouseOperatorId,
            LoadedOntoTruckId = _truckId,
            ActionType = "Loaded",
            NewCustodian = "María López"
        };
        _context.ShipmentCheckpoints.Add(checkpoint);

        // Actualizar estado
        shipment.Status = ShipmentStatus.Loaded;
        shipment.TruckId = _truckId;
        await _context.SaveChangesAsync();

        Assert.Equal(ShipmentStatus.Loaded, shipment.Status);
    }

    private async Task Step11_DriverPicksUpShipment()
    {
        // Checkpoint: Chofer escanea QR y toma custodia
        var checkpoint = new ShipmentCheckpoint
        {
            Id = Guid.NewGuid(),
            ShipmentId = _shipmentId,
            LocationId = _originLocationId,
            StatusCode = CheckpointStatus.QrScanned,
            Remarks = "Custodia transferida a chofer",
            Timestamp = DateTime.UtcNow.AddMinutes(30),
            CreatedByUserId = _driverUserId,
            HandledByDriverId = _driverId,
            LoadedOntoTruckId = _truckId,
            ActionType = "CustodyTransfer",
            PreviousCustodian = "María López",
            NewCustodian = "Carlos Rodríguez"
        };
        _context.ShipmentCheckpoints.Add(checkpoint);

        // Actualizar envío
        var shipment = await _context.Shipments.FindAsync(_shipmentId);
        shipment!.Status = ShipmentStatus.InTransit;
        shipment.DriverId = _driverId;
        shipment.WasQrScanned = true;
        shipment.AssignedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        Assert.Equal(ShipmentStatus.InTransit, shipment.Status);
    }

    private async Task Step12_ShipmentArrivesAtHub()
    {
        var checkpoint = new ShipmentCheckpoint
        {
            Id = Guid.NewGuid(),
            ShipmentId = _shipmentId,
            LocationId = _hubLocationId,
            StatusCode = CheckpointStatus.ArrivedHub,
            Remarks = "Llegada a Hub SLP",
            Timestamp = DateTime.UtcNow.AddHours(5),
            CreatedByUserId = _driverUserId,
            HandledByDriverId = _driverId,
            ActionType = "ArrivedHub"
        };
        _context.ShipmentCheckpoints.Add(checkpoint);

        var shipment = await _context.Shipments.FindAsync(_shipmentId);
        shipment!.Status = ShipmentStatus.AtHub;
        shipment.CurrentStepOrder = 2;
        await _context.SaveChangesAsync();

        Assert.Equal(ShipmentStatus.AtHub, shipment.Status);
    }

    private async Task Step13_ShipmentOutForDelivery()
    {
        var checkpoint = new ShipmentCheckpoint
        {
            Id = Guid.NewGuid(),
            ShipmentId = _shipmentId,
            LocationId = _hubLocationId,
            StatusCode = CheckpointStatus.DepartedHub,
            Remarks = "Salida hacia CDMX",
            Timestamp = DateTime.UtcNow.AddHours(6),
            CreatedByUserId = _driverUserId,
            HandledByDriverId = _driverId,
            ActionType = "DepartedHub"
        };
        _context.ShipmentCheckpoints.Add(checkpoint);

        // Cerca del destino
        var outForDelivery = new ShipmentCheckpoint
        {
            Id = Guid.NewGuid(),
            ShipmentId = _shipmentId,
            LocationId = _destLocationId,
            StatusCode = CheckpointStatus.OutForDelivery,
            Remarks = "En camino a punto de entrega",
            Timestamp = DateTime.UtcNow.AddHours(9),
            CreatedByUserId = _driverUserId,
            HandledByDriverId = _driverId,
            ActionType = "OutForDelivery"
        };
        _context.ShipmentCheckpoints.Add(outForDelivery);

        var shipment = await _context.Shipments.FindAsync(_shipmentId);
        shipment!.Status = ShipmentStatus.OutForDelivery;
        shipment.CurrentStepOrder = 3;
        await _context.SaveChangesAsync();
    }

    private async Task Step14_ShipmentDelivered()
    {
        var checkpoint = new ShipmentCheckpoint
        {
            Id = Guid.NewGuid(),
            ShipmentId = _shipmentId,
            LocationId = _destLocationId,
            StatusCode = CheckpointStatus.Delivered,
            Remarks = "Entrega exitosa. Firma: Laura Martínez",
            Timestamp = DateTime.UtcNow.AddHours(10),
            CreatedByUserId = _driverUserId,
            HandledByDriverId = _driverId,
            ActionType = "Delivered",
            NewCustodian = "Laura Martínez (Destinatario)"
        };
        _context.ShipmentCheckpoints.Add(checkpoint);

        var shipment = await _context.Shipments.FindAsync(_shipmentId);
        shipment!.Status = ShipmentStatus.Delivered;
        shipment.DeliveredAt = DateTime.UtcNow.AddHours(10);
        shipment.RecipientSignatureUrl = "/signatures/PAR-E2E001.png";
        await _context.SaveChangesAsync();

        Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
        Assert.NotNull(shipment.DeliveredAt);
    }

    // ==================== FASE 5: DOCUMENTOS ====================

    private async Task Step15_GenerateDocuments()
    {
        var documents = new[]
        {
            new ShipmentDocument
            {
                Id = Guid.NewGuid(),
                ShipmentId = _shipmentId,
                DocumentType = DocumentType.ServiceOrder,
                FileUrl = "/documents/PAR-E2E001/service_order.pdf",
                GeneratedBy = "System",
                GeneratedAt = DateTime.UtcNow.AddHours(-2)
            },
            new ShipmentDocument
            {
                Id = Guid.NewGuid(),
                ShipmentId = _shipmentId,
                DocumentType = DocumentType.Waybill,
                FileUrl = "/documents/PAR-E2E001/carta_porte.pdf",
                GeneratedBy = "System",
                GeneratedAt = DateTime.UtcNow.AddHours(-2)
            },
            new ShipmentDocument
            {
                Id = Guid.NewGuid(),
                ShipmentId = _shipmentId,
                DocumentType = DocumentType.Manifest,
                FileUrl = "/documents/PAR-E2E001/manifest.pdf",
                GeneratedBy = "System",
                GeneratedAt = DateTime.UtcNow.AddHours(-1)
            },
            new ShipmentDocument
            {
                Id = Guid.NewGuid(),
                ShipmentId = _shipmentId,
                DocumentType = DocumentType.POD,
                FileUrl = "/documents/PAR-E2E001/pod.pdf",
                GeneratedBy = "System",
                GeneratedAt = DateTime.UtcNow
            }
        };
        _context.ShipmentDocuments.AddRange(documents);
        await _context.SaveChangesAsync();

        Assert.Equal(4, await _context.ShipmentDocuments.CountAsync());
    }

    // ==================== VERIFICACIÓN FINAL ====================

    private async Task VerifyAllTablesHaveData()
    {
        // Verificar TODAS las tablas tienen datos
        var results = new Dictionary<string, int>
        {
            ["Tenants"] = await _context.Tenants.CountAsync(),
            ["Users"] = await _context.Users.CountAsync(),
            ["Roles"] = await _context.Roles.CountAsync(),
            ["Employees"] = await _context.Employees.CountAsync(),
            ["Shifts"] = await _context.Shifts.CountAsync(),
            ["Drivers"] = await _context.Drivers.CountAsync(),
            ["WarehouseOperators"] = await _context.WarehouseOperators.CountAsync(),
            ["Trucks"] = await _context.Trucks.CountAsync(),
            ["FleetLogs"] = await _context.FleetLogs.CountAsync(),
            ["Locations"] = await _context.Locations.CountAsync(),
            ["WarehouseZones"] = await _context.WarehouseZones.CountAsync(),
            ["NetworkLinks"] = await _context.NetworkLinks.CountAsync(),
            ["RouteBlueprints"] = await _context.RouteBlueprints.CountAsync(),
            ["RouteSteps"] = await _context.RouteSteps.CountAsync(),
            ["Clients"] = await _context.Clients.CountAsync(),
            ["Shipments"] = await _context.Shipments.CountAsync(),
            ["ShipmentItems"] = await _context.ShipmentItems.CountAsync(),
            ["ShipmentCheckpoints"] = await _context.ShipmentCheckpoints.CountAsync(),
            ["ShipmentDocuments"] = await _context.ShipmentDocuments.CountAsync()
        };

        // Verificar que TODAS las tablas tienen al menos 1 registro
        foreach (var (table, count) in results)
        {
            Assert.True(count > 0, $"La tabla {table} está vacía");
        }

        // Verificar conteos específicos
        Assert.Equal(4, results["Users"]); // SuperAdmin, Admin, Driver, Warehouse
        Assert.Equal(3, results["Employees"]); // Admin, Driver, Warehouse
        Assert.Equal(6, results["ShipmentCheckpoints"]); // Loaded, QR, ArrivedHub, Departed, OutForDelivery, Delivered
        Assert.Equal(4, results["ShipmentDocuments"]); // ServiceOrder, Waybill, Manifest, POD

        // Verificar estado final del envío
        var finalShipment = await _context.Shipments
            .Include(s => s.Items)
            .Include(s => s.History)
            .Include(s => s.Documents)
            .FirstOrDefaultAsync(s => s.Id == _shipmentId);

        Assert.NotNull(finalShipment);
        Assert.Equal(ShipmentStatus.Delivered, finalShipment.Status);
        Assert.Equal(2, finalShipment.Items.Count);
        Assert.Equal(6, finalShipment.History.Count);
        Assert.Equal(4, finalShipment.Documents.Count);
    }
}
