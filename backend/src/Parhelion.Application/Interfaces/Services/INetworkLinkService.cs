using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Network;
using Parhelion.Domain.Entities;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de enlaces de red logística.
/// </summary>
public interface INetworkLinkService : IGenericService<NetworkLink, NetworkLinkResponse, CreateNetworkLinkRequest, UpdateNetworkLinkRequest>
{
    /// <summary>
    /// Obtiene enlaces por tenant.
    /// </summary>
    Task<PagedResult<NetworkLinkResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene enlaces desde una ubicación de origen.
    /// </summary>
    Task<PagedResult<NetworkLinkResponse>> GetByOriginAsync(Guid originLocationId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene enlaces hacia una ubicación de destino.
    /// </summary>
    Task<PagedResult<NetworkLinkResponse>> GetByDestinationAsync(Guid destinationLocationId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene enlaces activos.
    /// </summary>
    Task<PagedResult<NetworkLinkResponse>> GetActiveAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);
}
