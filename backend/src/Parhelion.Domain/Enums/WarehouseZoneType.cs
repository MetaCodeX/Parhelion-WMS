namespace Parhelion.Domain.Enums;

/// <summary>
/// Tipo de zona dentro de una bodega/almacén.
/// </summary>
public enum WarehouseZoneType
{
    /// <summary>Área de recepción de mercancía</summary>
    Receiving,
    
    /// <summary>Almacenamiento general</summary>
    Storage,
    
    /// <summary>Área de preparación para despacho</summary>
    Staging,
    
    /// <summary>Andén de salida</summary>
    Shipping,
    
    /// <summary>Cuarto frío / Cadena de frío</summary>
    ColdChain,
    
    /// <summary>Materiales peligrosos</summary>
    Hazmat
}
