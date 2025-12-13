using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Parada individual en una ruta predefinida.
/// Incluye orden y tiempo de tránsito desde la parada anterior.
/// </summary>
public class RouteStep : BaseEntity
{
    public Guid RouteBlueprintId { get; set; }
    public Guid LocationId { get; set; }
    public int StepOrder { get; set; }
    public TimeSpan StandardTransitTime { get; set; }
    public RouteStepType StepType { get; set; }

    // Navigation Properties
    public RouteBlueprint RouteBlueprint { get; set; } = null!;
    public Location Location { get; set; } = null!;
}
