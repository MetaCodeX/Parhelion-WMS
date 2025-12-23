-- ================================================================
-- PARHELION WMS - Test Data for n8n Integration E2E (FIXED)
-- Tenant: "TransporteMX" con Admin y 3 Choferes con ubicaciones GPS
-- ================================================================

-- Insertar Drivers con nombres correctos de columnas
INSERT INTO "Drivers" ("Id", "TenantId", "LicenseNumber", "LicenseType", "LicenseExpiration", "Status", "DefaultTruckId", "CurrentTruckId", "UserId", "CreatedAt", "IsDeleted")
VALUES
    ('aaaaaaaa-bbbb-cccc-dddd-drvr00000001', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'LIC-MTY-001', 'Federal', '2026-12-31', 2, 'aaaaaaaa-bbbb-cccc-dddd-truck0000001', 'aaaaaaaa-bbbb-cccc-dddd-truck0000001', 'aaaaaaaa-bbbb-cccc-dddd-driver000001', NOW(), false),
    ('aaaaaaaa-bbbb-cccc-dddd-drvr00000002', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'LIC-MTY-002', 'Federal', '2027-06-30', 0, 'aaaaaaaa-bbbb-cccc-dddd-truck0000002', 'aaaaaaaa-bbbb-cccc-dddd-truck0000002', 'aaaaaaaa-bbbb-cccc-dddd-driver000002', NOW(), false),
    ('aaaaaaaa-bbbb-cccc-dddd-drvr00000003', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'LIC-GDL-003', 'Federal', '2025-08-15', 0, 'aaaaaaaa-bbbb-cccc-dddd-truck0000003', 'aaaaaaaa-bbbb-cccc-dddd-truck0000003', 'aaaaaaaa-bbbb-cccc-dddd-driver000003', NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Insertar Shipment
INSERT INTO "Shipments" ("Id", "TenantId", "TrackingNumber", "Status", "OriginLocationId", "DestinationLocationId", "TruckId", "DriverId", "TotalWeightKg", "TotalVolumeM3", "DeclaredValue", "ScheduledDeparture", "EstimatedArrival", "IsDelayed", "CreatedAt", "IsDeleted")
VALUES (
    'aaaabbbb-cccc-dddd-eeee-ffffffffffff',
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    'TRX-2025-001',
    5,
    'aaaaaaaa-bbbb-cccc-dddd-loc000000001',
    'aaaaaaaa-bbbb-cccc-dddd-loc000000002',
    'aaaaaaaa-bbbb-cccc-dddd-truck0000001',
    'aaaaaaaa-bbbb-cccc-dddd-drvr00000001',
    5000,
    20,
    150000.00,
    NOW() - INTERVAL '2 hours',
    NOW() + INTERVAL '10 hours',
    false,
    NOW(),
    false
)
ON CONFLICT ("Id") DO NOTHING;

-- Insertar ShipmentItem con dimensiones correctas
INSERT INTO "ShipmentItems" ("Id", "ShipmentId", "Description", "PackagingType", "Quantity", "WeightKg", "WidthCm", "HeightCm", "LengthCm", "DeclaredValue", "IsFragile", "IsHazardous", "RequiresRefrigeration", "CreatedAt", "IsDeleted")
VALUES (
    'aaaabbbb-cccc-dddd-eeee-111111111111',
    'aaaabbbb-cccc-dddd-eeee-ffffffffffff',
    'Electrodomésticos',
    0,
    50,
    5000,
    100,
    100,
    200,
    150000.00,
    false,
    false,
    false,
    NOW(),
    false
)
ON CONFLICT ("Id") DO NOTHING;

SELECT '=== DATOS ADICIONALES INSERTADOS ===' AS resultado;
SELECT 'Drivers: Juan(OnTrip), María(Available/Cerca), Pedro(Available/Lejos)' AS drivers;
SELECT 'Shipment: TRX-2025-001 (Status=InTransit)' AS shipment;
