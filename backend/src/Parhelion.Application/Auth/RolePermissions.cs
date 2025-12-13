using Parhelion.Domain.Enums;

namespace Parhelion.Application.Auth;

/// <summary>
/// Definición INMUTABLE de permisos por rol.
/// 
/// IMPORTANTE: Estos permisos están definidos en código.
/// Ni siquiera el Admin puede modificarlos en runtime.
/// Solo pueden cambiarse modificando el código fuente.
/// 
/// Esto garantiza que los permisos son consistentes y seguros
/// independientemente de lo que haya en la base de datos.
/// </summary>
public static class RolePermissions
{
    /// <summary>
    /// Permisos asignados a cada rol del sistema.
    /// </summary>
    private static readonly Dictionary<string, HashSet<Permission>> _permissions = new()
    {
        // ========== ADMIN ==========
        // Acceso total excepto modificar permisos de roles
        ["Admin"] = new HashSet<Permission>
        {
            // Usuarios
            Permission.UsersRead,
            Permission.UsersCreate,
            Permission.UsersUpdate,
            Permission.UsersDelete,
            
            // Camiones
            Permission.TrucksRead,
            Permission.TrucksCreate,
            Permission.TrucksUpdate,
            Permission.TrucksDelete,
            
            // Choferes
            Permission.DriversRead,
            Permission.DriversCreate,
            Permission.DriversUpdate,
            Permission.DriversDelete,
            
            // Clientes
            Permission.ClientsRead,
            Permission.ClientsCreate,
            Permission.ClientsUpdate,
            Permission.ClientsDelete,
            
            // Envíos
            Permission.ShipmentsRead,
            Permission.ShipmentsCreate,
            Permission.ShipmentsUpdate,
            Permission.ShipmentsDelete,
            Permission.ShipmentsAssign,
            
            // Items
            Permission.ShipmentItemsRead,
            Permission.ShipmentItemsCreate,
            Permission.ShipmentItemsUpdate,
            
            // Checkpoints
            Permission.CheckpointsRead,
            Permission.CheckpointsCreate,
            
            // Rutas
            Permission.RoutesRead,
            Permission.RoutesCreate,
            Permission.RoutesUpdate,
            Permission.RoutesDelete,
            
            // Ubicaciones
            Permission.LocationsRead,
            Permission.LocationsCreate,
            Permission.LocationsUpdate,
            Permission.LocationsDelete,
            
            // Documentos
            Permission.DocumentsRead,
            Permission.DocumentsCreate,
            
            // Fleet Logs
            Permission.FleetLogsRead,
            Permission.FleetLogsCreate
        },
        
        // ========== DRIVER (Chofer) ==========
        // Solo ve sus envíos asignados, puede crear checkpoints
        ["Driver"] = new HashSet<Permission>
        {
            Permission.ShipmentsReadOwn,
            Permission.CheckpointsCreate,
            Permission.DocumentsReadOwn,
            Permission.RoutesRead,
            Permission.LocationsRead
        },
        
        // ========== WAREHOUSE (Almacenista) ==========
        // Ve envíos de su ubicación, gestiona items y carga
        ["Warehouse"] = new HashSet<Permission>
        {
            Permission.ShipmentsReadByLocation,
            Permission.ShipmentsRead,
            Permission.ShipmentItemsRead,
            Permission.ShipmentItemsUpdate,
            Permission.CheckpointsCreate,
            Permission.TrucksRead,
            Permission.DriversRead,
            Permission.LocationsRead
        },
        
        // ========== DEMOUSER (Reclutador/Demo) ==========
        // Solo lectura de datos demo
        ["DemoUser"] = new HashSet<Permission>
        {
            Permission.UsersRead,
            Permission.TrucksRead,
            Permission.DriversRead,
            Permission.ClientsRead,
            Permission.ShipmentsRead,
            Permission.ShipmentItemsRead,
            Permission.CheckpointsRead,
            Permission.RoutesRead,
            Permission.LocationsRead,
            Permission.DocumentsRead,
            Permission.FleetLogsRead
        }
    };

    /// <summary>
    /// Verifica si un rol tiene un permiso específico.
    /// </summary>
    /// <param name="roleName">Nombre del rol (Admin, Driver, Warehouse, DemoUser)</param>
    /// <param name="permission">Permiso a verificar</param>
    /// <returns>True si el rol tiene el permiso</returns>
    public static bool HasPermission(string roleName, Permission permission)
    {
        return _permissions.TryGetValue(roleName, out var permissions) 
            && permissions.Contains(permission);
    }

    /// <summary>
    /// Obtiene todos los permisos de un rol.
    /// </summary>
    /// <param name="roleName">Nombre del rol</param>
    /// <returns>Conjunto de permisos (vacío si el rol no existe)</returns>
    public static IReadOnlySet<Permission> GetPermissions(string roleName)
    {
        return _permissions.TryGetValue(roleName, out var permissions)
            ? permissions
            : new HashSet<Permission>();
    }

    /// <summary>
    /// Obtiene todos los roles disponibles en el sistema.
    /// </summary>
    /// <returns>Lista de nombres de roles</returns>
    public static IEnumerable<string> GetAllRoles() => _permissions.Keys;

    /// <summary>
    /// Verifica si un rol existe.
    /// </summary>
    /// <param name="roleName">Nombre del rol</param>
    /// <returns>True si el rol existe</returns>
    public static bool RoleExists(string roleName) => _permissions.ContainsKey(roleName);
}
