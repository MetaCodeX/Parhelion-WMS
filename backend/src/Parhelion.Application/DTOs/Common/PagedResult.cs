namespace Parhelion.Application.DTOs.Common;

/// <summary>
/// Respuesta paginada genérica para listados.
/// Incluye metadata de paginación para frontend.
/// </summary>
/// <typeparam name="T">Tipo del DTO de cada item.</typeparam>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public PagedResult() { }

    public PagedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>
    /// Crea un resultado vacío.
    /// </summary>
    public static PagedResult<T> Empty(int page = 1, int pageSize = 20)
        => new(Enumerable.Empty<T>(), 0, page, pageSize);

    /// <summary>
    /// Crea resultado desde una lista ya paginada.
    /// </summary>
    public static PagedResult<T> From(IEnumerable<T> items, int totalCount, PagedRequest request)
        => new(items, totalCount, request.Page, request.PageSize);
}
