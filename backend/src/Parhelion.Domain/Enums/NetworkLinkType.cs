namespace Parhelion.Domain.Enums;

/// <summary>
/// Tipos de enlace en la red logística Hub &amp; Spoke.
/// </summary>
public enum NetworkLinkType
{
    /// <summary>Recolección: Cliente/Proveedor → Hub</summary>
    FirstMile,
    
    /// <summary>Carretera: Hub → Hub (larga distancia)</summary>
    LineHaul,
    
    /// <summary>Entrega: Hub → Cliente/Tienda</summary>
    LastMile
}
