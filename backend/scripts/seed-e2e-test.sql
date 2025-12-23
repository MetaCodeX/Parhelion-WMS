-- ================================================================
-- PARHELION WMS - Test Data for n8n Integration E2E
-- Tenant: "TransporteMX" con Admin y 3 Choferes con ubicaciones GPS
-- ================================================================

-- Primero limpiamos datos de prueba anteriores (si existen)
DELETE FROM "Notifications" WHERE "TenantId" = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee';
DELETE FROM "ShipmentItems" WHERE "ShipmentId" IN (SELECT "Id" FROM "Shipments" WHERE "TenantId" = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee');
DELETE FROM "Shipments" WHERE "TenantId" = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee';
DELETE FROM "FleetLogs" WHERE "TenantId" = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee';
DELETE FROM "Drivers" WHERE "TenantId" = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee';
DELETE FROM "Trucks" WHERE "TenantId" = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee';
DELETE FROM "Employees" WHERE "TenantId" = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee';
DELETE FROM "Locations" WHERE "TenantId" = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee';
DELETE FROM "Users" WHERE "TenantId" = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee';
DELETE FROM "ServiceApiKeys" WHERE "TenantId" = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee';
DELETE FROM "Tenants" WHERE "Id" = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee';

-- ================================================================
-- 1. TENANT
-- ================================================================
INSERT INTO "Tenants" ("Id", "CompanyName", "ContactEmail", "FleetSize", "DriverCount", "IsActive", "CreatedAt", "IsDeleted")
VALUES (
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    'TransporteMX',
    'admin@transportemx.com',
    5,
    3,
    true,
    NOW(),
    false
);

-- ================================================================
-- 2. SERVICE API KEY (para n8n callback - hasheada)
-- Hash of: 'test-n8n-key-transportemx-2025'
-- ================================================================
INSERT INTO "ServiceApiKeys" ("Id", "TenantId", "KeyHash", "Name", "Description", "IsActive", "CreatedAt", "IsDeleted")
VALUES (
    'aaaaaaaa-bbbb-cccc-dddd-111111111111',
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    encode(sha256('test-n8n-key-transportemx-2025'::bytea), 'hex'),
    'n8n-transportemx-test',
    'API Key de prueba para test E2E',
    true,
    NOW(),
    false
);

-- ================================================================
-- 3. ROLE (Admin)
-- ================================================================
-- Usamos el rol Admin que ya debería existir
-- Si no existe, crearlo:
INSERT INTO "Roles" ("Id", "Name", "Description", "CreatedAt", "IsDeleted")
VALUES ('11111111-1111-1111-1111-111111111111', 'Admin', 'Administrator', NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Roles" ("Id", "Name", "Description", "CreatedAt", "IsDeleted")
VALUES ('22222222-2222-2222-2222-222222222222', 'Driver', 'Chofer', NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- ================================================================
-- 4. USERS (1 Admin + 3 Drivers)
-- Password: Test1234! (bcrypt hash)
-- ================================================================
-- Admin
INSERT INTO "Users" ("Id", "TenantId", "Email", "PasswordHash", "FullName", "RoleId", "IsDemoUser", "IsActive", "CreatedAt", "IsDeleted")
VALUES (
    'aaaaaaaa-bbbb-cccc-dddd-admin0000001',
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    'admin@transportemx.com',
    '$2a$11$K.0HwPqDcBH3V5yJ8EQR.eL1K4F5nC3.V5vI4n1tA5m6O3r7R9s0e', -- Test1234!
    'Carlos Admin',
    '11111111-1111-1111-1111-111111111111',
    false,
    true,
    NOW(),
    false
);

-- Driver 1: Juan (EN MONTERREY - el que tendrá el problema)
INSERT INTO "Users" ("Id", "TenantId", "Email", "PasswordHash", "FullName", "RoleId", "IsDemoUser", "IsActive", "CreatedAt", "IsDeleted")
VALUES (
    'aaaaaaaa-bbbb-cccc-dddd-driver000001',
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    'juan@transportemx.com',
    '$2a$11$K.0HwPqDcBH3V5yJ8EQR.eL1K4F5nC3.V5vI4n1tA5m6O3r7R9s0e',
    'Juan Pérez (Afectado)',
    '22222222-2222-2222-2222-222222222222',
    false,
    true,
    NOW(),
    false
);

-- Driver 2: María (CERCA DE MONTERREY - 10km - rescatista cercana)
INSERT INTO "Users" ("Id", "TenantId", "Email", "PasswordHash", "FullName", "RoleId", "IsDemoUser", "IsActive", "CreatedAt", "IsDeleted")
VALUES (
    'aaaaaaaa-bbbb-cccc-dddd-driver000002',
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    'maria@transportemx.com',
    '$2a$11$K.0HwPqDcBH3V5yJ8EQR.eL1K4F5nC3.V5vI4n1tA5m6O3r7R9s0e',
    'María García (Rescatista Cercana)',
    '22222222-2222-2222-2222-222222222222',
    false,
    true,
    NOW(),
    false
);

-- Driver 3: Pedro (LEJOS - Guadalajara - 500km)
INSERT INTO "Users" ("Id", "TenantId", "Email", "PasswordHash", "FullName", "RoleId", "IsDemoUser", "IsActive", "CreatedAt", "IsDeleted")
VALUES (
    'aaaaaaaa-bbbb-cccc-dddd-driver000003',
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    'pedro@transportemx.com',
    '$2a$11$K.0HwPqDcBH3V5yJ8EQR.eL1K4F5nC3.V5vI4n1tA5m6O3r7R9s0e',
    'Pedro Ramírez (Lejano)',
    '22222222-2222-2222-2222-222222222222',
    false,
    true,
    NOW(),
    false
);

-- ================================================================
-- 5. EMPLOYEES
-- ================================================================
INSERT INTO "Employees" ("Id", "TenantId", "FirstName", "LastName", "Position", "Email", "Phone", "HireDate", "IsActive", "UserId", "CreatedAt", "IsDeleted")
VALUES
    ('aaaaaaaa-bbbb-cccc-dddd-empl00000001', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'Juan', 'Pérez', 'Chofer', 'juan@transportemx.com', '+52 81 1234 5678', '2023-01-15', true, 'aaaaaaaa-bbbb-cccc-dddd-driver000001', NOW(), false),
    ('aaaaaaaa-bbbb-cccc-dddd-empl00000002', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'María', 'García', 'Chofer', 'maria@transportemx.com', '+52 81 2345 6789', '2023-03-20', true, 'aaaaaaaa-bbbb-cccc-dddd-driver000002', NOW(), false),
    ('aaaaaaaa-bbbb-cccc-dddd-empl00000003', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'Pedro', 'Ramírez', 'Chofer', 'pedro@transportemx.com', '+52 33 3456 7890', '2023-06-01', true, 'aaaaaaaa-bbbb-cccc-dddd-driver000003', NOW(), false);

-- ================================================================
-- 6. LOCATIONS (Hub en Monterrey, destino en CDMX)
-- ================================================================
INSERT INTO "Locations" ("Id", "TenantId", "Code", "Name", "Type", "Latitude", "Longitude", "IsActive", "CreatedAt", "IsDeleted")
VALUES
    ('aaaaaaaa-bbbb-cccc-dddd-loc000000001', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'MTY-HUB', 'Hub Monterrey', 0, 25.6866, -100.3161, true, NOW(), false),
    ('aaaaaaaa-bbbb-cccc-dddd-loc000000002', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'CDMX-DST', 'Destino CDMX', 3, 19.4326, -99.1332, true, NOW(), false);

-- ================================================================
-- 7. TRUCKS (3 camiones con ubicaciones GPS)
-- ================================================================
-- Camión 1: Juan (EN MONTERREY CENTRO - el averiado)
INSERT INTO "Trucks" ("Id", "TenantId", "PlateNumber", "Type", "MaxCapacityKg", "MaxVolumeM3", "Model", "Year", "IsActive", "LastLatitude", "LastLongitude", "LastLocationUpdate", "CreatedAt", "IsDeleted")
VALUES (
    'aaaaaaaa-bbbb-cccc-dddd-truck0000001',
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    'MTY-001-AAA',
    0, -- DryBox
    10000,
    40,
    'Kenworth T680',
    2022,
    true,
    25.6750,    -- Lat: Centro Monterrey (punto de avería)
    -100.3100,  -- Lon
    NOW(),
    NOW(),
    false
);

-- Camión 2: María (10km al norte de Monterrey - CERCANA y DISPONIBLE)
INSERT INTO "Trucks" ("Id", "TenantId", "PlateNumber", "Type", "MaxCapacityKg", "MaxVolumeM3", "Model", "Year", "IsActive", "LastLatitude", "LastLongitude", "LastLocationUpdate", "CreatedAt", "IsDeleted")
VALUES (
    'aaaaaaaa-bbbb-cccc-dddd-truck0000002',
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    'MTY-002-BBB',
    0, -- DryBox
    10000,
    40,
    'Freightliner Cascadia',
    2023,
    true,
    25.7700,    -- Lat: ~10km norte de MTY
    -100.3000,  -- Lon
    NOW(),
    NOW(),
    false
);

-- Camión 3: Pedro (Guadalajara - LEJANO)
INSERT INTO "Trucks" ("Id", "TenantId", "PlateNumber", "Type", "MaxCapacityKg", "MaxVolumeM3", "Model", "Year", "IsActive", "LastLatitude", "LastLongitude", "LastLocationUpdate", "CreatedAt", "IsDeleted")
VALUES (
    'aaaaaaaa-bbbb-cccc-dddd-truck0000003',
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    'GDL-003-CCC',
    0, -- DryBox
    10000,
    40,
    'Volvo VNL 860',
    2021,
    true,
    20.6597,    -- Lat: Guadalajara
    -103.3496,  -- Lon
    NOW(),
    NOW(),
    false
);

-- ================================================================
-- 8. DRIVERS (con status y asignaciones)
-- ================================================================
-- Juan: Status=OnTrip (está trabajando, tendrá la avería)
INSERT INTO "Drivers" ("Id", "TenantId", "LicenseNumber", "LicenseExpiry", "Status", "DefaultTruckId", "CurrentTruckId", "UserId", "CreatedAt", "IsDeleted")
VALUES (
    'aaaaaaaa-bbbb-cccc-dddd-drvr00000001',
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    'LIC-MTY-001',
    '2026-12-31',
    2, -- OnTrip
    'aaaaaaaa-bbbb-cccc-dddd-truck0000001',
    'aaaaaaaa-bbbb-cccc-dddd-truck0000001',
    'aaaaaaaa-bbbb-cccc-dddd-driver000001',
    NOW(),
    false
);

-- María: Status=Available (DISPONIBLE para rescate)
INSERT INTO "Drivers" ("Id", "TenantId", "LicenseNumber", "LicenseExpiry", "Status", "DefaultTruckId", "CurrentTruckId", "UserId", "CreatedAt", "IsDeleted")
VALUES (
    'aaaaaaaa-bbbb-cccc-dddd-drvr00000002',
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    'LIC-MTY-002',
    '2027-06-30',
    0, -- Available ← Esta es la que debe encontrar n8n
    'aaaaaaaa-bbbb-cccc-dddd-truck0000002',
    'aaaaaaaa-bbbb-cccc-dddd-truck0000002',
    'aaaaaaaa-bbbb-cccc-dddd-driver000002',
    NOW(),
    false
);

-- Pedro: Status=Available pero lejano
INSERT INTO "Drivers" ("Id", "TenantId", "LicenseNumber", "LicenseExpiry", "Status", "DefaultTruckId", "CurrentTruckId", "UserId", "CreatedAt", "IsDeleted")
VALUES (
    'aaaaaaaa-bbbb-cccc-dddd-drvr00000003',
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    'LIC-GDL-003',
    '2025-08-15',
    0, -- Available pero lejos
    'aaaaaaaa-bbbb-cccc-dddd-truck0000003',
    'aaaaaaaa-bbbb-cccc-dddd-truck0000003',
    'aaaaaaaa-bbbb-cccc-dddd-driver000003',
    NOW(),
    false
);

-- ================================================================
-- 9. SHIPMENT (El envío que tendrá la avería)
-- Status: InTransit (para que pueda cambiar a Exception)
-- ================================================================
INSERT INTO "Shipments" ("Id", "TenantId", "TrackingNumber", "Status", "OriginLocationId", "DestinationLocationId", "TruckId", "DriverId", "TotalWeightKg", "TotalVolumeM3", "DeclaredValue", "ScheduledDeparture", "EstimatedArrival", "IsDelayed", "CreatedAt", "IsDeleted")
VALUES (
    'aaaaaaaa-bbbb-cccc-dddd-ship00000001',
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    'TRX-2025-001',
    5, -- InTransit (puede cambiar a Exception)
    'aaaaaaaa-bbbb-cccc-dddd-loc000000001', -- MTY Hub
    'aaaaaaaa-bbbb-cccc-dddd-loc000000002', -- CDMX Destino
    'aaaaaaaa-bbbb-cccc-dddd-truck0000001', -- Camión de Juan
    'aaaaaaaa-bbbb-cccc-dddd-drvr00000001', -- Juan (el afectado)
    5000,
    20,
    150000.00,
    NOW() - INTERVAL '2 hours',
    NOW() + INTERVAL '10 hours',
    false,
    NOW(),
    false
);

-- Item del shipment
INSERT INTO "ShipmentItems" ("Id", "ShipmentId", "Description", "Quantity", "WeightKg", "VolumeM3", "DeclaredValue", "RequiresRefrigeration", "IsHazardous", "CreatedAt", "IsDeleted")
VALUES (
    'aaaaaaaa-bbbb-cccc-dddd-item00000001',
    'aaaaaaaa-bbbb-cccc-dddd-ship00000001',
    'Electrodomésticos',
    50,
    5000,
    20,
    150000.00,
    false,
    false,
    NOW(),
    false
);

-- ================================================================
-- RESULTADOS ESPERADOS:
-- ================================================================
-- Cuando el Shipment TRX-2025-001 cambie a Exception:
-- 1. Se publica webhook con coordenadas (25.6750, -100.3100)
-- 2. n8n busca en GET /api/drivers/nearby?lat=25.6750&lon=-100.31&radiusKm=50
-- 3. Debe encontrar a María (~10km) como la más cercana Y disponible
-- 4. n8n envía 2 notificaciones:
--    - A Juan (afectado): "Tu envío ha sido marcado como excepción"
--    - A María (rescatista): "Se te asignó apoyo para TRX-2025-001"
-- ================================================================

SELECT 'Datos de prueba insertados correctamente' AS resultado;
SELECT 'Tenant: TransporteMX (aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee)' AS info;
SELECT 'Shipment: TRX-2025-001 (Status: InTransit → cambiar a Exception para trigger)' AS test;
SELECT 'API Key de prueba: test-n8n-key-transportemx-2025' AS n8n_key;
