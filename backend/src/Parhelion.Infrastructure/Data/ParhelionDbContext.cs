using Microsoft.EntityFrameworkCore;
using Parhelion.Domain.Common;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data;

/// <summary>
/// DbContext principal de Parhelion Logistics.
/// Implementa Query Filters globales para:
/// - Multi-tenancy: Todas las entidades TenantEntity filtran por TenantId
/// - Soft Delete: Todas las entidades filtran por IsDeleted = false
/// </summary>
public class ParhelionDbContext : DbContext
{
    private readonly Guid? _tenantId;

    public ParhelionDbContext(DbContextOptions<ParhelionDbContext> options) 
        : base(options)
    {
    }

    /// <summary>
    /// Constructor con tenant ID para multi-tenancy.
    /// El TenantId se inyecta desde el middleware/servicio de autenticación.
    /// </summary>
    public ParhelionDbContext(DbContextOptions<ParhelionDbContext> options, Guid? tenantId) 
        : base(options)
    {
        _tenantId = tenantId;
    }

    // ========== DbSets ==========
    
    // Core
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    // Clientes (remitentes/destinatarios)
    public DbSet<Client> Clients => Set<Client>();
    
    // Flotilla
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Truck> Trucks => Set<Truck>();
    public DbSet<FleetLog> FleetLogs => Set<FleetLog>();
    
    // Empleados y Turnos (v0.4.3)
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Shift> Shifts => Set<Shift>();
    
    // Almacén (v0.4.3)
    public DbSet<WarehouseZone> WarehouseZones => Set<WarehouseZone>();
    public DbSet<WarehouseOperator> WarehouseOperators => Set<WarehouseOperator>();
    
    // Red Logística
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<NetworkLink> NetworkLinks => Set<NetworkLink>();
    public DbSet<RouteBlueprint> RouteBlueprints => Set<RouteBlueprint>();
    public DbSet<RouteStep> RouteSteps => Set<RouteStep>();
    
    // Envíos
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();
    public DbSet<ShipmentCheckpoint> ShipmentCheckpoints => Set<ShipmentCheckpoint>();
    public DbSet<ShipmentDocument> ShipmentDocuments => Set<ShipmentDocument>();
    
    // Inventario y Catálogo (v0.4.4)
    public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();
    public DbSet<InventoryStock> InventoryStocks => Set<InventoryStock>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    
    // Notificaciones (Agentes IA n8n)
    public DbSet<Notification> Notifications => Set<Notification>();
    
    // Service API Keys (autenticación de servicios externos)
    public DbSet<ServiceApiKey> ServiceApiKeys => Set<ServiceApiKey>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones de Fluent API desde el assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ParhelionDbContext).Assembly);

        // ========== QUERY FILTERS GLOBALES ==========
        
        // Soft Delete filter para TODAS las entidades que heredan de BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ParhelionDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
                    .MakeGenericMethod(entityType.ClrType);
                
                method?.Invoke(null, new object[] { modelBuilder });
            }
        }

        // Multi-Tenant filter para entidades TenantEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType) && !entityType.IsOwned())
            {
                var method = typeof(ParhelionDbContext)
                    .GetMethod(nameof(SetTenantFilter), 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .MakeGenericMethod(entityType.ClrType);
                
                method?.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    /// <summary>
    /// Aplica filtro de Soft Delete a una entidad.
    /// </summary>
    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) 
        where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    /// <summary>
    /// Aplica filtro de Multi-Tenant a una entidad.
    /// </summary>
    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder) 
        where TEntity : TenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted && (_tenantId == null || e.TenantId == _tenantId));
    }

    /// <summary>
    /// Override de SaveChanges para Audit Trail automático.
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override de SaveChangesAsync para Audit Trail automático.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Actualiza automáticamente CreatedAt, UpdatedAt, DeletedAt.
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.IsDeleted = false;
                    break;
                    
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    
                    // Si se está eliminando lógicamente
                    if (entry.Entity.IsDeleted && entry.Entity.DeletedAt == null)
                    {
                        entry.Entity.DeletedAt = now;
                    }
                    break;
            }
        }
    }
}
