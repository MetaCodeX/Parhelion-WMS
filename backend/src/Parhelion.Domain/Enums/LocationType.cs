namespace Parhelion.Domain.Enums;

/// <summary>
/// Tipos de ubicación en la red logística.
/// </summary>
public enum LocationType
{
    /// <summary>Nodo central, recibe y despacha masivo</summary>
    RegionalHub,
    
    /// <summary>Transferencia rápida sin almacenamiento</summary>
    CrossDock,
    
    /// <summary>Bodega de almacenamiento prolongado</summary>
    Warehouse,
    
    /// <summary>Punto de venta final, solo recibe</summary>
    Store,
    
    /// <summary>Fábrica de origen, solo despacha</summary>
    SupplierPlant
}
