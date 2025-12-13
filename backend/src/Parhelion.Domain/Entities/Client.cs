using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Cliente (remitente o destinatario de envíos).
/// Representa a las empresas o personas que envían o reciben paquetes.
/// </summary>
public class Client : TenantEntity
{
    // ========== DATOS BÁSICOS ==========
    
    /// <summary>Nombre de la empresa</summary>
    public string CompanyName { get; set; } = null!;
    
    /// <summary>Nombre comercial (opcional)</summary>
    public string? TradeName { get; set; }
    
    /// <summary>Nombre del contacto principal</summary>
    public string ContactName { get; set; } = null!;
    
    /// <summary>Email de contacto</summary>
    public string Email { get; set; } = null!;
    
    /// <summary>Teléfono de contacto</summary>
    public string Phone { get; set; } = null!;
    
    // ========== DATOS FISCALES ==========
    
    /// <summary>RFC para facturación (México)</summary>
    public string? TaxId { get; set; }
    
    /// <summary>Razón Social para facturación</summary>
    public string? LegalName { get; set; }
    
    /// <summary>Dirección fiscal para facturación</summary>
    public string? BillingAddress { get; set; }
    
    // ========== DATOS DE ENVÍO ==========
    
    /// <summary>Dirección de envío/recepción predeterminada</summary>
    public string ShippingAddress { get; set; } = null!;
    
    /// <summary>Tipos de productos que suele enviar/recibir (ej: "Electrónicos, Frágiles")</summary>
    public string? PreferredProductTypes { get; set; }
    
    /// <summary>Prioridad del cliente para atención</summary>
    public ClientPriority Priority { get; set; } = ClientPriority.Normal;
    
    // ========== ESTADO ==========
    
    /// <summary>Si el cliente está activo</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>Notas internas sobre el cliente</summary>
    public string? Notes { get; set; }
    
    // ========== NAVIGATION PROPERTIES ==========
    
    /// <summary>Tenant al que pertenece este cliente</summary>
    public Tenant Tenant { get; set; } = null!;
    
    /// <summary>Envíos donde este cliente es el remitente</summary>
    public ICollection<Shipment> ShipmentsAsSender { get; set; } = new List<Shipment>();
    
    /// <summary>Envíos donde este cliente es el destinatario</summary>
    public ICollection<Shipment> ShipmentsAsRecipient { get; set; } = new List<Shipment>();
}
