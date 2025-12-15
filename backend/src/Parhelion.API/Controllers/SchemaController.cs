using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Expone metadatos del schema de base de datos para herramientas de administración.
/// Este endpoint es de solo lectura y no expone datos, solo estructura.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SchemaController : ControllerBase
{
    private readonly ParhelionDbContext _context;
    private static readonly object _cacheLock = new();
    private static SchemaMetadataResponse? _cachedSchema;
    private static DateTime _cacheExpiry = DateTime.MinValue;
    private const int CacheTtlMinutes = 60; // Cache por 1 hora

    public SchemaController(ParhelionDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene metadatos del schema de base de datos.
    /// Información pública: nombres de tablas, columnas y relaciones.
    /// No expone datos sensibles ni requiere autenticación.
    /// </summary>
    [HttpGet("metadata")]
    [ResponseCache(Duration = 3600)] // Browser cache 1 hora
    public ActionResult<SchemaMetadataResponse> GetSchemaMetadata()
    {
        // Check cache
        lock (_cacheLock)
        {
            if (_cachedSchema != null && DateTime.UtcNow < _cacheExpiry)
            {
                return Ok(_cachedSchema);
            }
        }

        var schema = BuildSchemaFromEfCore();
        
        // Update cache
        lock (_cacheLock)
        {
            _cachedSchema = schema;
            _cacheExpiry = DateTime.UtcNow.AddMinutes(CacheTtlMinutes);
        }

        return Ok(schema);
    }

    /// <summary>
    /// Fuerza recarga del cache de schema (requiere autenticación en producción).
    /// </summary>
    [HttpPost("refresh")]
    public ActionResult RefreshCache()
    {
        lock (_cacheLock)
        {
            _cachedSchema = null;
            _cacheExpiry = DateTime.MinValue;
        }
        return Ok(new { message = "Schema cache cleared" });
    }

    private SchemaMetadataResponse BuildSchemaFromEfCore()
    {
        var tables = new List<TableMetadata>();
        var model = _context.Model;

        // Categorización por módulo
        var moduleMapping = new Dictionary<string, string>
        {
            // Core
            { "Tenant", "core" },
            { "User", "core" },
            { "Role", "core" },
            { "RefreshToken", "core" },
            { "Client", "core" },
            
            // Employee
            { "Employee", "employee" },
            { "Shift", "employee" },
            
            // Fleet
            { "Driver", "fleet" },
            { "Truck", "fleet" },
            { "FleetLog", "fleet" },
            
            // Warehouse
            { "Location", "warehouse" },
            { "WarehouseZone", "warehouse" },
            { "WarehouseOperator", "warehouse" },
            
            // Inventory
            { "CatalogItem", "inventory" },
            { "InventoryStock", "inventory" },
            { "InventoryTransaction", "inventory" },
            
            // Shipment
            { "Shipment", "shipment" },
            { "ShipmentItem", "shipment" },
            { "ShipmentCheckpoint", "shipment" },
            { "ShipmentDocument", "shipment" },
            
            // Network/Routing
            { "NetworkLink", "network" },
            { "RouteBlueprint", "network" },
            { "RouteStep", "network" }
        };

        var descriptions = new Dictionary<string, string>
        {
            { "Tenant", "Multi-tenant root entity" },
            { "User", "System users with roles" },
            { "Role", "Admin, Driver, Warehouse, Demo" },
            { "RefreshToken", "JWT refresh tokens" },
            { "Client", "B2B clients (senders/recipients)" },
            { "Employee", "Employee profiles (v0.4.3)" },
            { "Shift", "Work shifts configuration" },
            { "Driver", "Fleet drivers with MX legal data" },
            { "Truck", "DryBox, Refrigerated, HAZMAT..." },
            { "FleetLog", "Driver-Truck changes log" },
            { "Location", "Hubs, Warehouses, Cross-docks" },
            { "WarehouseZone", "Zones within locations" },
            { "WarehouseOperator", "Operators assigned to zones" },
            { "CatalogItem", "Product catalog (v0.4.4)" },
            { "InventoryStock", "Stock by zone/lot (v0.4.4)" },
            { "InventoryTransaction", "Kardex movements (v0.4.4)" },
            { "Shipment", "Shipments PAR-XXXXXX" },
            { "ShipmentItem", "Items with volumetric weight" },
            { "ShipmentCheckpoint", "Immutable tracking events" },
            { "ShipmentDocument", "B2B docs: Waybill, POD..." },
            { "NetworkLink", "FirstMile, LineHaul, LastMile" },
            { "RouteBlueprint", "Predefined Hub & Spoke routes" },
            { "RouteStep", "Route stops with transit times" }
        };

        // Posiciones para layout visual (grid layout)
        var positions = new Dictionary<string, (int x, int y)>
        {
            // Row 1: Core
            { "Tenant", (50, 50) },
            { "Role", (280, 50) },
            { "User", (510, 50) },
            { "RefreshToken", (740, 50) },
            
            // Row 2: Employee + Client
            { "Employee", (50, 300) },
            { "Shift", (280, 300) },
            { "Client", (510, 300) },
            
            // Row 3: Fleet
            { "Truck", (50, 550) },
            { "Driver", (280, 550) },
            { "FleetLog", (510, 550) },
            
            // Row 4: Warehouse + Inventory (RIGHT SIDE)
            { "Location", (970, 50) },
            { "WarehouseZone", (1200, 50) },
            { "WarehouseOperator", (970, 300) },
            { "CatalogItem", (1200, 300) },
            { "InventoryStock", (970, 550) },
            { "InventoryTransaction", (1200, 550) },
            
            // Row 5: Shipment (BOTTOM)
            { "Shipment", (50, 800) },
            { "ShipmentItem", (280, 800) },
            { "ShipmentCheckpoint", (510, 800) },
            { "ShipmentDocument", (740, 800) },
            
            // Row 6: Network (BOTTOM RIGHT)
            { "NetworkLink", (970, 800) },
            { "RouteBlueprint", (1200, 800) },
            { "RouteStep", (1430, 800) }
        };

        foreach (var entityType in model.GetEntityTypes())
        {
            var entityName = entityType.ClrType.Name;
            
            // Skip owned types
            if (entityType.IsOwned()) continue;

            var fields = new List<FieldMetadata>();

            foreach (var property in entityType.GetProperties())
            {
                var isPk = property.IsPrimaryKey();
                var isFk = property.IsForeignKey();
                string? fkTarget = null;

                if (isFk)
                {
                    var fkEntity = property.GetContainingForeignKeys()
                        .FirstOrDefault()?.PrincipalEntityType?.ClrType.Name;
                    fkTarget = fkEntity != null ? $"{fkEntity}s" : null;
                }

                fields.Add(new FieldMetadata
                {
                    Name = property.Name,
                    Pk = isPk,
                    Fk = fkTarget,
                    Type = GetSimpleTypeName(property.ClrType),
                    IsNullable = property.IsNullable
                });
            }

            var pos = positions.GetValueOrDefault(entityName, (50, 50));
            
            tables.Add(new TableMetadata
            {
                Name = $"{entityName}s", // Pluralize
                Type = moduleMapping.GetValueOrDefault(entityName, "core"),
                Description = descriptions.GetValueOrDefault(entityName, entityName),
                X = pos.Item1,
                Y = pos.Item2,
                Fields = fields
            });
        }

        return new SchemaMetadataResponse
        {
            Version = "0.4.5",
            GeneratedAt = DateTime.UtcNow,
            TableCount = tables.Count,
            Tables = tables.OrderBy(t => t.Type).ThenBy(t => t.Name).ToList()
        };
    }

    private static string GetSimpleTypeName(Type type)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlying)
            type = underlying;

        return type.Name switch
        {
            nameof(Guid) => "uuid",
            nameof(String) => "string",
            nameof(Int32) => "int",
            nameof(Int64) => "long",
            nameof(Decimal) => "decimal",
            nameof(Boolean) => "bool",
            nameof(DateTime) => "datetime",
            nameof(TimeSpan) => "timespan",
            _ when type.IsEnum => "enum",
            _ => type.Name.ToLower()
        };
    }
}

// DTOs
public record SchemaMetadataResponse
{
    public string Version { get; init; } = "";
    public DateTime GeneratedAt { get; init; }
    public int TableCount { get; init; }
    public List<TableMetadata> Tables { get; init; } = new();
}

public record TableMetadata
{
    public string Name { get; init; } = "";
    public string Type { get; init; } = "";
    public string Description { get; init; } = "";
    public int X { get; init; }
    public int Y { get; init; }
    public List<FieldMetadata> Fields { get; init; } = new();
}

public record FieldMetadata
{
    public string Name { get; init; } = "";
    public bool Pk { get; init; }
    public string? Fk { get; init; }
    public string Type { get; init; } = "";
    public bool IsNullable { get; init; }
}
