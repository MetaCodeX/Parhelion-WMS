namespace Parhelion.Application.DTOs.Common;

/// <summary>
/// DTO base con campos de auditoría comunes.
/// </summary>
public abstract class BaseDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO base para entidades multi-tenant.
/// </summary>
public abstract class TenantDto : BaseDto
{
    public Guid TenantId { get; set; }
}

/// <summary>
/// Respuesta estándar de operación exitosa.
/// </summary>
public class OperationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? Id { get; set; }

    public static OperationResult Ok(string? message = null) 
        => new() { Success = true, Message = message };
    
    public static OperationResult Ok(Guid id, string? message = null) 
        => new() { Success = true, Id = id, Message = message };
    
    public static OperationResult Fail(string message) 
        => new() { Success = false, Message = message };
}

/// <summary>
/// Respuesta estándar con datos.
/// </summary>
public class OperationResult<T> : OperationResult
{
    public T? Data { get; set; }

    public static OperationResult<T> Ok(T data, string? message = null) 
        => new() { Success = true, Data = data, Message = message };
    
    public new static OperationResult<T> Fail(string message) 
        => new() { Success = false, Message = message };
}
