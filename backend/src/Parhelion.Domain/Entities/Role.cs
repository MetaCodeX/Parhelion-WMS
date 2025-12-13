using Parhelion.Domain.Common;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Roles del sistema: Admin, Driver, DemoUser.
/// No son multi-tenant (compartidos globalmente).
/// </summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Navigation Properties
    public ICollection<User> Users { get; set; } = new List<User>();
}
