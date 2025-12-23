using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parhelion.Application.DTOs.Webhooks;
using Parhelion.Application.Interfaces;

namespace Parhelion.Infrastructure.External.Webhooks;

/// <summary>
/// Implementación de IWebhookPublisher que envía eventos a n8n.
/// 
/// Características:
/// - Fire-and-forget: errores se loguean pero no interrumpen el flujo
/// - Configurable: se habilita/deshabilita via appsettings
/// - CallbackToken: cada webhook incluye un JWT firmado para autenticación de retorno
/// </summary>
public class N8nWebhookPublisher : IWebhookPublisher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<N8nWebhookPublisher> _logger;
    private readonly N8nConfiguration _config;
    private readonly ICallbackTokenService _tokenService;

    public N8nWebhookPublisher(
        HttpClient httpClient,
        ILogger<N8nWebhookPublisher> logger,
        IOptions<N8nConfiguration> config,
        ICallbackTokenService tokenService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config.Value;
        _tokenService = tokenService;
        
        // Configurar timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
        
        // Configurar API Key si existe (para autenticación con n8n)
        if (!string.IsNullOrEmpty(_config.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", _config.ApiKey);
        }
    }

    /// <inheritdoc />
    public async Task PublishAsync<T>(string eventType, T payload, CancellationToken cancellationToken = default) 
        where T : class
    {
        // Si está deshabilitado, salir silenciosamente
        if (!_config.Enabled)
        {
            _logger.LogDebug("Webhook disabled, skipping event {EventType}", eventType);
            return;
        }

        // Obtener URL del webhook
        var webhookUrl = _config.GetWebhookUrl(eventType);
        if (webhookUrl == null)
        {
            _logger.LogDebug("No webhook configured for event {EventType}", eventType);
            return;
        }

        try
        {
            var correlationId = Guid.NewGuid();
            
            // Extraer TenantId del payload (todos los eventos deben tenerlo)
            var tenantId = ExtractTenantId(payload);
            
            // Generar CallbackToken para autenticación de n8n
            string callbackToken;
            if (tenantId.HasValue)
            {
                callbackToken = _tokenService.GenerateCallbackToken(tenantId.Value, correlationId);
            }
            else
            {
                // Si no hay TenantId, generar token sin tenant (para eventos globales)
                callbackToken = _tokenService.GenerateCallbackToken(Guid.Empty, correlationId);
                _logger.LogWarning("Payload for {EventType} has no TenantId, using empty tenant", eventType);
            }

            // Crear envelope con metadatos y CallbackToken
            var envelope = new WebhookEvent(
                EventType: eventType,
                Timestamp: DateTime.UtcNow,
                CorrelationId: correlationId,
                CallbackToken: callbackToken,
                Payload: payload
            );

            // Enviar request HTTP POST
            var response = await _httpClient.PostAsJsonAsync(webhookUrl, envelope, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Webhook sent successfully: {EventType} to {Url} (Status: {StatusCode}, CorrelationId: {CorrelationId})", 
                    eventType, webhookUrl, (int)response.StatusCode, correlationId);
            }
            else
            {
                _logger.LogWarning(
                    "Webhook returned non-success status: {EventType} to {Url} (Status: {StatusCode})", 
                    eventType, webhookUrl, (int)response.StatusCode);
            }
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken != cancellationToken)
        {
            // Timeout
            _logger.LogWarning(
                "Webhook timeout: {EventType} to {Url} (Timeout: {Timeout}s)", 
                eventType, webhookUrl, _config.TimeoutSeconds);
        }
        catch (HttpRequestException ex)
        {
            // Error de red
            _logger.LogWarning(ex,
                "Webhook HTTP error: {EventType} to {Url}", 
                eventType, webhookUrl);
        }
        catch (Exception ex)
        {
            // Cualquier otro error
            _logger.LogWarning(ex,
                "Webhook unexpected error: {EventType} to {Url}", 
                eventType, webhookUrl);
        }
        
        // NOTA: No lanzamos excepciones - fire-and-forget
    }
    
    /// <summary>
    /// Extrae TenantId del payload usando reflection.
    /// Todos los eventos webhook deben tener una propiedad TenantId.
    /// </summary>
    private static Guid? ExtractTenantId<T>(T payload) where T : class
    {
        var property = typeof(T).GetProperty("TenantId");
        if (property?.PropertyType == typeof(Guid))
        {
            return (Guid?)property.GetValue(payload);
        }
        return null;
    }
}
