namespace Parhelion.Domain.Enums;

/// <summary>
/// Tipos de documento B2B para envíos.
/// </summary>
public enum DocumentType
{
    /// <summary>Orden de Servicio - Petición inicial de traslado</summary>
    ServiceOrder,
    
    /// <summary>Carta Porte - Documento legal SAT para inspecciones</summary>
    Waybill,
    
    /// <summary>Manifiesto - Checklist de carga con instrucciones</summary>
    Manifest,
    
    /// <summary>Hoja de Ruta - Itinerario con ventanas de entrega</summary>
    TripSheet,
    
    /// <summary>Prueba de Entrega - Firma digital del receptor</summary>
    POD
}
