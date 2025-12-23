using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Services.Core;

/// <summary>
/// Implementación del servicio de Tenants.
/// Gestiona operaciones CRUD para empresas clientes del sistema multi-tenant.
/// </summary>
public class TenantService : ITenantService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Inicializa una nueva instancia del servicio de Tenants.
    /// </summary>
    /// <param name="unitOfWork">Unit of Work para coordinación de repositorios.</param>
    public TenantService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<PagedResult<TenantResponse>> GetAllAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Tenants.GetPagedAsync(
            request,
            filter: null,
            orderBy: q => q.OrderByDescending(t => t.CreatedAt),
            cancellationToken);

        var dtos = items.Select(t => MapToResponse(t));
        return PagedResult<TenantResponse>.From(dtos, totalCount, request);
    }

    /// <inheritdoc />
    public async Task<TenantResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Tenants.GetByIdAsync(id, cancellationToken);
        return entity != null ? MapToResponse(entity) : null;
    }

    /// <inheritdoc />
    public async Task<OperationResult<TenantResponse>> CreateAsync(
        CreateTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validar email único
        var existingByEmail = await _unitOfWork.Tenants.FirstOrDefaultAsync(
            t => t.ContactEmail == request.ContactEmail, cancellationToken);
        
        if (existingByEmail != null)
        {
            return OperationResult<TenantResponse>.Fail(
                $"Ya existe un tenant con el email '{request.ContactEmail}'");
        }

        var entity = new Tenant
        {
            Id = Guid.NewGuid(),
            CompanyName = request.CompanyName,
            ContactEmail = request.ContactEmail,
            FleetSize = request.FleetSize,
            DriverCount = request.DriverCount,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Tenants.AddAsync(entity, cancellationToken);
        
        // Generar ServiceApiKey automáticamente para el nuevo tenant
        var (plainTextKey, apiKey) = GenerateServiceApiKey(entity.Id, entity.CompanyName);
        await _unitOfWork.ServiceApiKeys.AddAsync(apiKey, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<TenantResponse>.Ok(
            MapToResponse(entity, plainTextKey),
            "Tenant creado exitosamente. IMPORTANTE: Guarda la API Key, no podrás verla de nuevo.");
    }
    
    /// <summary>
    /// Genera una ServiceApiKey segura para un tenant.
    /// Retorna (plainTextKey, entity) - La key en texto plano solo se muestra una vez.
    /// </summary>
    private static (string PlainTextKey, ServiceApiKey Entity) GenerateServiceApiKey(Guid tenantId, string tenantName)
    {
        // Generar key aleatoria segura: prefix + random bytes
        var randomBytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(randomBytes);
        var plainTextKey = $"pk_{tenantName.ToLowerInvariant().Replace(" ", "")[..Math.Min(6, tenantName.Length)]}_{Convert.ToBase64String(randomBytes).Replace("+", "").Replace("/", "").Replace("=", "")[..32]}";
        
        // Hash SHA256 para almacenamiento seguro
        var keyHash = ComputeSha256Hash(plainTextKey);
        
        var apiKey = new ServiceApiKey
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            KeyHash = keyHash,
            Name = $"n8n-{tenantName.ToLowerInvariant().Replace(" ", "-")}",
            Description = "API Key generada automáticamente para integración n8n",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        return (plainTextKey, apiKey);
    }
    
    /// <summary>
    /// Computa SHA256 hash de la key para almacenamiento seguro.
    /// </summary>
    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawData));
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }

    /// <inheritdoc />
    public async Task<OperationResult<TenantResponse>> UpdateAsync(
        Guid id,
        UpdateTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Tenants.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return OperationResult<TenantResponse>.Fail("Tenant no encontrado");
        }

        // Validar email único (excluyendo el actual)
        var existingByEmail = await _unitOfWork.Tenants.FirstOrDefaultAsync(
            t => t.ContactEmail == request.ContactEmail && t.Id != id, cancellationToken);
        
        if (existingByEmail != null)
        {
            return OperationResult<TenantResponse>.Fail(
                $"Ya existe otro tenant con el email '{request.ContactEmail}'");
        }

        entity.CompanyName = request.CompanyName;
        entity.ContactEmail = request.ContactEmail;
        entity.FleetSize = request.FleetSize;
        entity.DriverCount = request.DriverCount;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Tenants.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<TenantResponse>.Ok(
            MapToResponse(entity),
            "Tenant actualizado exitosamente");
    }

    /// <inheritdoc />
    public async Task<OperationResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Tenants.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return OperationResult.Fail("Tenant no encontrado");
        }

        _unitOfWork.Tenants.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult.Ok("Tenant eliminado exitosamente");
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Tenants.AnyAsync(t => t.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TenantResponse?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Tenants.FirstOrDefaultAsync(
            t => t.ContactEmail == email, cancellationToken);
        return entity != null ? MapToResponse(entity) : null;
    }

    /// <inheritdoc />
    public async Task<PagedResult<TenantResponse>> GetActiveAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Tenants.GetPagedAsync(
            request,
            filter: t => t.IsActive,
            orderBy: q => q.OrderByDescending(t => t.CreatedAt),
            cancellationToken);

        var dtos = items.Select(t => MapToResponse(t));
        return PagedResult<TenantResponse>.From(dtos, totalCount, request);
    }

    /// <inheritdoc />
    public async Task<OperationResult> SetActiveStatusAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Tenants.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return OperationResult.Fail("Tenant no encontrado");
        }

        entity.IsActive = isActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Tenants.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var status = isActive ? "activado" : "desactivado";
        return OperationResult.Ok($"Tenant {status} exitosamente");
    }

    /// <summary>
    /// Mapea una entidad Tenant a su DTO de respuesta.
    /// </summary>
    private static TenantResponse MapToResponse(Tenant entity, string? generatedApiKey = null) => new(
        entity.Id,
        entity.CompanyName,
        entity.ContactEmail,
        entity.FleetSize,
        entity.DriverCount,
        entity.IsActive,
        entity.CreatedAt,
        entity.UpdatedAt,
        generatedApiKey
    );
}
