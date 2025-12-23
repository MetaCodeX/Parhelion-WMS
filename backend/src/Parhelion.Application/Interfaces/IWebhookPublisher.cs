namespace Parhelion.Application.Interfaces;

/// <summary>
/// Interfaz para publicar eventos hacia sistemas externos (n8n, webhooks, etc.).
/// Diseñada para ser fire-and-forget: errores se loguean pero no interrumpen el flujo principal.
/// </summary>
public interface IWebhookPublisher
{
    /// <summary>
    /// Publica un evento tipado hacia el sistema de webhooks configurado.
    /// </summary>
    /// <typeparam name="T">Tipo del payload del evento.</typeparam>
    /// <param name="eventType">Identificador del evento (ej: "shipment.exception").</param>
    /// <param name="payload">Datos del evento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task PublishAsync<T>(string eventType, T payload, CancellationToken cancellationToken = default) 
        where T : class;
}

/// <summary>
/// Implementación nula de IWebhookPublisher para cuando los webhooks están desactivados.
/// Permite inyección de dependencias sin configuración adicional.
/// </summary>
public class NullWebhookPublisher : IWebhookPublisher
{
    public Task PublishAsync<T>(string eventType, T payload, CancellationToken cancellationToken = default) 
        where T : class => Task.CompletedTask;
}
