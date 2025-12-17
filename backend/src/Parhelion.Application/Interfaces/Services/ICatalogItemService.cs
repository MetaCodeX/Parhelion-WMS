using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Catalog;
using Parhelion.Domain.Entities;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de CatalogItems (catálogo maestro de productos).
/// Maneja productos con SKU, dimensiones y características especiales.
/// </summary>
public interface ICatalogItemService : IGenericService<CatalogItem, CatalogItemResponse, CreateCatalogItemRequest, UpdateCatalogItemRequest>
{
    /// <summary>
    /// Busca un producto por SKU.
    /// </summary>
    /// <param name="tenantId">ID del tenant.</param>
    /// <param name="sku">SKU del producto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Producto encontrado o null.</returns>
    Task<CatalogItemResponse?> GetBySkuAsync(
        Guid tenantId,
        string sku,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene productos por tenant.
    /// </summary>
    /// <param name="tenantId">ID del tenant.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de productos del tenant.</returns>
    Task<PagedResult<CatalogItemResponse>> GetByTenantAsync(
        Guid tenantId,
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca productos por nombre o descripción.
    /// </summary>
    /// <param name="tenantId">ID del tenant.</param>
    /// <param name="searchTerm">Término de búsqueda.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de productos que coinciden.</returns>
    Task<PagedResult<CatalogItemResponse>> SearchAsync(
        Guid tenantId,
        string searchTerm,
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene productos que requieren refrigeración.
    /// </summary>
    /// <param name="tenantId">ID del tenant.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de productos refrigerados.</returns>
    Task<PagedResult<CatalogItemResponse>> GetRefrigeratedAsync(
        Guid tenantId,
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene productos peligrosos (HAZMAT).
    /// </summary>
    /// <param name="tenantId">ID del tenant.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de productos peligrosos.</returns>
    Task<PagedResult<CatalogItemResponse>> GetHazardousAsync(
        Guid tenantId,
        PagedRequest request,
        CancellationToken cancellationToken = default);
}
