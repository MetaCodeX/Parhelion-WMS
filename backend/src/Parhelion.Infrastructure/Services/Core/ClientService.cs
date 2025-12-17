using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Services.Core;

/// <summary>
/// Implementación del servicio de Clients.
/// Gestiona clientes B2B (remitentes/destinatarios de envíos).
/// </summary>
public class ClientService : IClientService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Inicializa una nueva instancia del servicio de Clients.
    /// </summary>
    /// <param name="unitOfWork">Unit of Work para coordinación de repositorios.</param>
    public ClientService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<PagedResult<ClientResponse>> GetAllAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Clients.GetPagedAsync(
            request,
            filter: null,
            orderBy: q => q.OrderByDescending(c => c.CreatedAt),
            cancellationToken);

        var dtos = items.Select(MapToResponse);
        return PagedResult<ClientResponse>.From(dtos, totalCount, request);
    }

    /// <inheritdoc />
    public async Task<ClientResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Clients.GetByIdAsync(id, cancellationToken);
        return entity != null ? MapToResponse(entity) : null;
    }

    /// <inheritdoc />
    public async Task<OperationResult<ClientResponse>> CreateAsync(
        CreateClientRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validar email único
        var existingByEmail = await _unitOfWork.Clients.FirstOrDefaultAsync(
            c => c.Email == request.Email, cancellationToken);
        
        if (existingByEmail != null)
        {
            return OperationResult<ClientResponse>.Fail(
                $"Ya existe un cliente con el email '{request.Email}'");
        }

        // Validar Tax ID único si se proporciona
        if (!string.IsNullOrEmpty(request.TaxId))
        {
            var existingByTaxId = await _unitOfWork.Clients.FirstOrDefaultAsync(
                c => c.TaxId == request.TaxId, cancellationToken);
            
            if (existingByTaxId != null)
            {
                return OperationResult<ClientResponse>.Fail(
                    $"Ya existe un cliente con el RFC '{request.TaxId}'");
            }
        }

        // Parsear prioridad
        if (!Enum.TryParse<ClientPriority>(request.Priority, out var priority))
        {
            priority = ClientPriority.Normal;
        }

        var entity = new Client
        {
            Id = Guid.NewGuid(),
            CompanyName = request.CompanyName,
            TradeName = request.TradeName,
            ContactName = request.ContactName,
            Email = request.Email,
            Phone = request.Phone,
            TaxId = request.TaxId,
            LegalName = request.LegalName,
            BillingAddress = request.BillingAddress,
            ShippingAddress = request.ShippingAddress,
            PreferredProductTypes = request.PreferredProductTypes,
            Priority = priority,
            IsActive = true,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Clients.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<ClientResponse>.Ok(
            MapToResponse(entity),
            "Cliente creado exitosamente");
    }

    /// <inheritdoc />
    public async Task<OperationResult<ClientResponse>> UpdateAsync(
        Guid id,
        UpdateClientRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Clients.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return OperationResult<ClientResponse>.Fail("Cliente no encontrado");
        }

        // Validar email único (excluyendo el actual)
        var existingByEmail = await _unitOfWork.Clients.FirstOrDefaultAsync(
            c => c.Email == request.Email && c.Id != id, cancellationToken);
        
        if (existingByEmail != null)
        {
            return OperationResult<ClientResponse>.Fail(
                $"Ya existe otro cliente con el email '{request.Email}'");
        }

        // Validar Tax ID único (excluyendo el actual)
        if (!string.IsNullOrEmpty(request.TaxId))
        {
            var existingByTaxId = await _unitOfWork.Clients.FirstOrDefaultAsync(
                c => c.TaxId == request.TaxId && c.Id != id, cancellationToken);
            
            if (existingByTaxId != null)
            {
                return OperationResult<ClientResponse>.Fail(
                    $"Ya existe otro cliente con el RFC '{request.TaxId}'");
            }
        }

        // Parsear prioridad
        if (!Enum.TryParse<ClientPriority>(request.Priority, out var priority))
        {
            priority = entity.Priority;
        }

        entity.CompanyName = request.CompanyName;
        entity.TradeName = request.TradeName;
        entity.ContactName = request.ContactName;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.TaxId = request.TaxId;
        entity.LegalName = request.LegalName;
        entity.BillingAddress = request.BillingAddress;
        entity.ShippingAddress = request.ShippingAddress;
        entity.PreferredProductTypes = request.PreferredProductTypes;
        entity.Priority = priority;
        entity.IsActive = request.IsActive;
        entity.Notes = request.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Clients.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<ClientResponse>.Ok(
            MapToResponse(entity),
            "Cliente actualizado exitosamente");
    }

    /// <inheritdoc />
    public async Task<OperationResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Clients.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return OperationResult.Fail("Cliente no encontrado");
        }

        _unitOfWork.Clients.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult.Ok("Cliente eliminado exitosamente");
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Clients.AnyAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ClientResponse?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Clients.FirstOrDefaultAsync(
            c => c.Email == email, cancellationToken);
        return entity != null ? MapToResponse(entity) : null;
    }

    /// <inheritdoc />
    public async Task<ClientResponse?> GetByTaxIdAsync(
        string taxId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Clients.FirstOrDefaultAsync(
            c => c.TaxId == taxId, cancellationToken);
        return entity != null ? MapToResponse(entity) : null;
    }

    /// <inheritdoc />
    public async Task<PagedResult<ClientResponse>> GetByTenantAsync(
        Guid tenantId,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Clients.GetPagedAsync(
            request,
            filter: c => c.TenantId == tenantId,
            orderBy: q => q.OrderByDescending(c => c.CreatedAt),
            cancellationToken);

        var dtos = items.Select(MapToResponse);
        return PagedResult<ClientResponse>.From(dtos, totalCount, request);
    }

    /// <inheritdoc />
    public async Task<PagedResult<ClientResponse>> GetByPriorityAsync(
        Guid tenantId,
        ClientPriority priority,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Clients.GetPagedAsync(
            request,
            filter: c => c.TenantId == tenantId && c.Priority == priority,
            orderBy: q => q.OrderByDescending(c => c.CreatedAt),
            cancellationToken);

        var dtos = items.Select(MapToResponse);
        return PagedResult<ClientResponse>.From(dtos, totalCount, request);
    }

    /// <inheritdoc />
    public async Task<PagedResult<ClientResponse>> SearchByCompanyNameAsync(
        Guid tenantId,
        string companyName,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var searchTerm = companyName.ToLower();
        var (items, totalCount) = await _unitOfWork.Clients.GetPagedAsync(
            request,
            filter: c => c.TenantId == tenantId && 
                        c.CompanyName.ToLower().Contains(searchTerm),
            orderBy: q => q.OrderByDescending(c => c.CreatedAt),
            cancellationToken);

        var dtos = items.Select(MapToResponse);
        return PagedResult<ClientResponse>.From(dtos, totalCount, request);
    }

    /// <summary>
    /// Mapea una entidad Client a su DTO de respuesta.
    /// </summary>
    private static ClientResponse MapToResponse(Client entity) => new(
        entity.Id,
        entity.CompanyName,
        entity.TradeName,
        entity.ContactName,
        entity.Email,
        entity.Phone,
        entity.TaxId,
        entity.LegalName,
        entity.BillingAddress,
        entity.ShippingAddress,
        entity.PreferredProductTypes,
        entity.Priority.ToString(),
        entity.IsActive,
        entity.Notes,
        entity.CreatedAt,
        entity.UpdatedAt
    );
}
