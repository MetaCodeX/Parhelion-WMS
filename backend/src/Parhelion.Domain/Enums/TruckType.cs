namespace Parhelion.Domain.Enums;

/// <summary>
/// Tipos de camión con requisitos especiales de carga.
/// </summary>
public enum TruckType
{
    /// <summary>Caja Seca - Carga estándar: cartón, ropa, electrónica</summary>
    DryBox,
    
    /// <summary>Termo/Refrigerado - Cadena de frío: alimentos, farmacéuticos</summary>
    Refrigerated,
    
    /// <summary>Pipa HAZMAT - Materiales peligrosos: químicos, combustible</summary>
    HazmatTank,
    
    /// <summary>Plataforma - Carga pesada: acero, maquinaria, construcción</summary>
    Flatbed,
    
    /// <summary>Blindado - Alto valor: electrónicos, valores, dinero</summary>
    Armored
}
