using Parhelion.Domain.Common;
using Parhelion.Domain.Entities;

namespace Parhelion.Application.Interfaces;

/// <summary>
/// Unit of Work para coordinar transacciones entre repositorios.
/// Garantiza que múltiples operaciones se confirmen o reviertan juntas.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // ========== CORE REPOSITORIES ==========
    IGenericRepository<Tenant> Tenants { get; }
    ITenantRepository<User> Users { get; }
    IGenericRepository<Role> Roles { get; }
    ITenantRepository<Employee> Employees { get; }
    ITenantRepository<Client> Clients { get; }
    
    // ========== FLEET REPOSITORIES ==========
    ITenantRepository<Truck> Trucks { get; }
    IGenericRepository<Driver> Drivers { get; }
    ITenantRepository<Shift> Shifts { get; }
    ITenantRepository<FleetLog> FleetLogs { get; }
    
    // ========== WAREHOUSE REPOSITORIES ==========
    ITenantRepository<Location> Locations { get; }
    IGenericRepository<WarehouseZone> WarehouseZones { get; }
    IGenericRepository<WarehouseOperator> WarehouseOperators { get; }
    ITenantRepository<InventoryStock> InventoryStocks { get; }
    ITenantRepository<InventoryTransaction> InventoryTransactions { get; }
    ITenantRepository<CatalogItem> CatalogItems { get; }
    
    // ========== SHIPMENT REPOSITORIES ==========
    ITenantRepository<Shipment> Shipments { get; }
    IGenericRepository<ShipmentItem> ShipmentItems { get; }
    IGenericRepository<ShipmentCheckpoint> ShipmentCheckpoints { get; }
    IGenericRepository<ShipmentDocument> ShipmentDocuments { get; }
    
    // ========== NETWORK REPOSITORIES ==========
    ITenantRepository<NetworkLink> NetworkLinks { get; }
    ITenantRepository<RouteBlueprint> RouteBlueprints { get; }
    IGenericRepository<RouteStep> RouteSteps { get; }
    
    // ========== NOTIFICATION / N8N REPOSITORIES ==========
    IGenericRepository<Notification> Notifications { get; }
    ITenantRepository<ServiceApiKey> ServiceApiKeys { get; }
    
    // ========== TRANSACTION CONTROL ==========
    
    /// <summary>
    /// Guarda todos los cambios pendientes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Inicia una transacción explícita.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Confirma la transacción actual.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revierte la transacción actual.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
