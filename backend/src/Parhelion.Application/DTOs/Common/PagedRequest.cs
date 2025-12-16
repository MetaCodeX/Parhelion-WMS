namespace Parhelion.Application.DTOs.Common;

/// <summary>
/// Request para paginación y ordenamiento de listados.
/// Usado en todos los endpoints GET que retornan colecciones.
/// </summary>
public class PagedRequest
{
    private int _page = 1;
    private int _pageSize = 20;
    private const int MaxPageSize = 100;

    /// <summary>
    /// Número de página (1-indexed).
    /// </summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Cantidad de registros por página (max 100).
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
    }

    /// <summary>
    /// Campo por el cual ordenar (ej: "CreatedAt", "Name").
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// True para orden descendente.
    /// </summary>
    public bool SortDescending { get; set; }

    /// <summary>
    /// Búsqueda de texto libre (aplica a campos específicos por entidad).
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filtrar solo activos (IsActive = true, IsDeleted = false).
    /// Default: true
    /// </summary>
    public bool ActiveOnly { get; set; } = true;

    /// <summary>
    /// Calcula el offset para skip en querys.
    /// </summary>
    public int Skip => (Page - 1) * PageSize;
}
