using Parhelion.Application.DTOs.Common;
using Parhelion.Domain.Common;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Interface base genérica para servicios CRUD.
/// Proporciona operaciones estándar para todas las entidades del dominio.
/// </summary>
/// <typeparam name="TEntity">Tipo de entidad del dominio.</typeparam>
/// <typeparam name="TResponse">DTO de respuesta.</typeparam>
/// <typeparam name="TCreateRequest">DTO de creación.</typeparam>
/// <typeparam name="TUpdateRequest">DTO de actualización.</typeparam>
public interface IGenericService<TEntity, TResponse, TCreateRequest, TUpdateRequest>
    where TEntity : BaseEntity
{
    /// <summary>
    /// Obtiene entidades paginadas.
    /// </summary>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado con DTOs de respuesta.</returns>
    Task<PagedResult<TResponse>> GetAllAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una entidad por su ID.
    /// </summary>
    /// <param name="id">ID de la entidad.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>DTO de respuesta o null si no existe.</returns>
    Task<TResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea una nueva entidad.
    /// </summary>
    /// <param name="request">DTO con datos de creación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado de la operación con el ID creado.</returns>
    Task<OperationResult<TResponse>> CreateAsync(
        TCreateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una entidad existente.
    /// </summary>
    /// <param name="id">ID de la entidad a actualizar.</param>
    /// <param name="request">DTO con datos de actualización.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<OperationResult<TResponse>> UpdateAsync(
        Guid id,
        TUpdateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una entidad (soft delete).
    /// </summary>
    /// <param name="id">ID de la entidad a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<OperationResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe una entidad con el ID especificado.
    /// </summary>
    /// <param name="id">ID a verificar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>True si existe, false en caso contrario.</returns>
    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
