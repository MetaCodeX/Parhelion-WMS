using Microsoft.EntityFrameworkCore.Storage;
using Parhelion.Application.Interfaces;
using Parhelion.Domain.Entities;
using Parhelion.Infrastructure.Data;

namespace Parhelion.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation.
/// Coordina repositorios y transacciones.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ParhelionDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    
    // Lazy-loaded repositories
    private IGenericRepository<Tenant>? _tenants;
    private ITenantRepository<User>? _users;
    private IGenericRepository<Role>? _roles;
    private ITenantRepository<Employee>? _employees;
    private ITenantRepository<Client>? _clients;
    
    private ITenantRepository<Truck>? _trucks;
    private IGenericRepository<Driver>? _drivers;
    private ITenantRepository<Shift>? _shifts;
    private ITenantRepository<FleetLog>? _fleetLogs;
    
    private ITenantRepository<Location>? _locations;
    private IGenericRepository<WarehouseZone>? _warehouseZones;
    private IGenericRepository<WarehouseOperator>? _warehouseOperators;
    private ITenantRepository<InventoryStock>? _inventoryStocks;
    private ITenantRepository<InventoryTransaction>? _inventoryTransactions;
    private ITenantRepository<CatalogItem>? _catalogItems;
    
    private ITenantRepository<Shipment>? _shipments;
    private IGenericRepository<ShipmentItem>? _shipmentItems;
    private IGenericRepository<ShipmentCheckpoint>? _shipmentCheckpoints;
    private IGenericRepository<ShipmentDocument>? _shipmentDocuments;
    
    private ITenantRepository<NetworkLink>? _networkLinks;
    private ITenantRepository<RouteBlueprint>? _routeBlueprints;
    private IGenericRepository<RouteStep>? _routeSteps;

    public UnitOfWork(ParhelionDbContext context)
    {
        _context = context;
    }

    // ========== CORE REPOSITORIES ==========
    
    public IGenericRepository<Tenant> Tenants => 
        _tenants ??= new GenericRepository<Tenant>(_context);
    
    public ITenantRepository<User> Users => 
        _users ??= new TenantRepository<User>(_context);
    
    public IGenericRepository<Role> Roles => 
        _roles ??= new GenericRepository<Role>(_context);
    
    public ITenantRepository<Employee> Employees => 
        _employees ??= new TenantRepository<Employee>(_context);
    
    public ITenantRepository<Client> Clients => 
        _clients ??= new TenantRepository<Client>(_context);

    // ========== FLEET REPOSITORIES ==========
    
    public ITenantRepository<Truck> Trucks => 
        _trucks ??= new TenantRepository<Truck>(_context);
    
    public IGenericRepository<Driver> Drivers => 
        _drivers ??= new GenericRepository<Driver>(_context);
    
    public ITenantRepository<Shift> Shifts => 
        _shifts ??= new TenantRepository<Shift>(_context);
    
    public ITenantRepository<FleetLog> FleetLogs => 
        _fleetLogs ??= new TenantRepository<FleetLog>(_context);

    // ========== WAREHOUSE REPOSITORIES ==========
    
    public ITenantRepository<Location> Locations => 
        _locations ??= new TenantRepository<Location>(_context);
    
    public IGenericRepository<WarehouseZone> WarehouseZones => 
        _warehouseZones ??= new GenericRepository<WarehouseZone>(_context);
    
    public IGenericRepository<WarehouseOperator> WarehouseOperators => 
        _warehouseOperators ??= new GenericRepository<WarehouseOperator>(_context);
    
    public ITenantRepository<InventoryStock> InventoryStocks => 
        _inventoryStocks ??= new TenantRepository<InventoryStock>(_context);
    
    public ITenantRepository<InventoryTransaction> InventoryTransactions => 
        _inventoryTransactions ??= new TenantRepository<InventoryTransaction>(_context);
    
    public ITenantRepository<CatalogItem> CatalogItems => 
        _catalogItems ??= new TenantRepository<CatalogItem>(_context);

    // ========== SHIPMENT REPOSITORIES ==========
    
    public ITenantRepository<Shipment> Shipments => 
        _shipments ??= new TenantRepository<Shipment>(_context);
    
    public IGenericRepository<ShipmentItem> ShipmentItems => 
        _shipmentItems ??= new GenericRepository<ShipmentItem>(_context);
    
    public IGenericRepository<ShipmentCheckpoint> ShipmentCheckpoints => 
        _shipmentCheckpoints ??= new GenericRepository<ShipmentCheckpoint>(_context);
    
    public IGenericRepository<ShipmentDocument> ShipmentDocuments => 
        _shipmentDocuments ??= new GenericRepository<ShipmentDocument>(_context);

    // ========== NETWORK REPOSITORIES ==========
    
    public ITenantRepository<NetworkLink> NetworkLinks => 
        _networkLinks ??= new TenantRepository<NetworkLink>(_context);
    
    public ITenantRepository<RouteBlueprint> RouteBlueprints => 
        _routeBlueprints ??= new TenantRepository<RouteBlueprint>(_context);
    
    public IGenericRepository<RouteStep> RouteSteps => 
        _routeSteps ??= new GenericRepository<RouteStep>(_context);

    // ========== TRANSACTION CONTROL ==========

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.CommitAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    // ========== DISPOSE ==========

    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _currentTransaction?.Dispose();
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
