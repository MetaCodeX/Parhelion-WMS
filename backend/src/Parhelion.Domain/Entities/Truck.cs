using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Camión de la flotilla con capacidad máxima en kg y volumen en m³.
/// El tipo de camión determina qué mercancía puede transportar.
/// </summary>
public class Truck : TenantEntity
{
    // ========== DATOS BÁSICOS ==========
    
    public string Plate { get; set; } = null!;
    public string Model { get; set; } = null!;
    public TruckType Type { get; set; }
    public decimal MaxCapacityKg { get; set; }
    public decimal MaxVolumeM3 { get; set; }
    public bool IsActive { get; set; }
    
    // ========== DATOS DEL VEHÍCULO ==========
    
    /// <summary>Número de Identificación Vehicular (VIN/Serie)</summary>
    public string? Vin { get; set; }
    
    /// <summary>Número de motor</summary>
    public string? EngineNumber { get; set; }
    
    /// <summary>Año del vehículo</summary>
    public int? Year { get; set; }
    
    /// <summary>Color del vehículo</summary>
    public string? Color { get; set; }
    
    // ========== DOCUMENTACIÓN LEGAL ==========
    
    /// <summary>Número de póliza de seguro</summary>
    public string? InsurancePolicy { get; set; }
    
    /// <summary>Fecha de vencimiento del seguro</summary>
    public DateTime? InsuranceExpiration { get; set; }
    
    /// <summary>Número de verificación vehicular</summary>
    public string? VerificationNumber { get; set; }
    
    /// <summary>Fecha de vencimiento de verificación</summary>
    public DateTime? VerificationExpiration { get; set; }
    
    // ========== MANTENIMIENTO ==========
    
    /// <summary>Fecha del último mantenimiento</summary>
    public DateTime? LastMaintenanceDate { get; set; }
    
    /// <summary>Próximo mantenimiento programado</summary>
    public DateTime? NextMaintenanceDate { get; set; }
    
    /// <summary>Odómetro actual en kilómetros</summary>
    public decimal? CurrentOdometerKm { get; set; }

    // ========== TELEMETRÍA (GPS) ==========
    
    /// <summary>Última latitud reportada</summary>
    public decimal? LastLatitude { get; set; }
    
    /// <summary>Última longitud reportada</summary>
    public decimal? LastLongitude { get; set; }
    
    /// <summary>Fecha del último reporte de ubicación</summary>
    public DateTime? LastLocationUpdate { get; set; }

    // ========== NAVIGATION PROPERTIES ==========
    
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    
    /// <summary>
    /// Choferes que tienen este camión como asignación fija.
    /// </summary>
    public ICollection<Driver> DefaultDrivers { get; set; } = new List<Driver>();
    
    /// <summary>
    /// Choferes que actualmente conducen este camión.
    /// </summary>
    public ICollection<Driver> CurrentDrivers { get; set; } = new List<Driver>();
    
    public ICollection<FleetLog> OldTruckLogs { get; set; } = new List<FleetLog>();
    public ICollection<FleetLog> NewTruckLogs { get; set; } = new List<FleetLog>();
    public ICollection<ShipmentCheckpoint> LoadedCheckpoints { get; set; } = new List<ShipmentCheckpoint>();
}
