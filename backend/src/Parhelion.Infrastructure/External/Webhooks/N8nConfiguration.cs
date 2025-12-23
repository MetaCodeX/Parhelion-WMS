namespace Parhelion.Infrastructure.External.Webhooks;

/// <summary>
/// Configuración para el servicio de webhooks n8n.
/// Se carga desde appsettings.json sección "N8n".
/// </summary>
public class N8nConfiguration
{
    /// <summary>Habilita/deshabilita el envío de webhooks (default: false)</summary>
    public bool Enabled { get; set; } = false;
    
    /// <summary>URL base de n8n (ej: "http://n8n:5678" o "http://localhost:5678")</summary>
    public string BaseUrl { get; set; } = "http://localhost:5678";
    
    /// <summary>API Key opcional para autenticación con n8n</summary>
    public string? ApiKey { get; set; }
    
    /// <summary>Timeout en segundos para requests HTTP (default: 10)</summary>
    public int TimeoutSeconds { get; set; } = 10;
    
    /// <summary>
    /// Mapeo de tipos de evento a rutas de webhook.
    /// Key: EventType (ej: "shipment.exception")
    /// Value: Path del webhook (ej: "/webhook/shipment-exception")
    /// </summary>
    public Dictionary<string, string> Webhooks { get; set; } = new();
    
    /// <summary>
    /// Obtiene la URL completa del webhook para un tipo de evento.
    /// </summary>
    /// <param name="eventType">Tipo de evento (ej: "shipment.exception")</param>
    /// <returns>URL completa o null si no está configurado</returns>
    public string? GetWebhookUrl(string eventType)
    {
        if (!Webhooks.TryGetValue(eventType, out var path))
            return null;
        
        var baseUrl = BaseUrl.TrimEnd('/');
        var webhookPath = path.StartsWith('/') ? path : $"/{path}";
        return $"{baseUrl}{webhookPath}";
    }
}
