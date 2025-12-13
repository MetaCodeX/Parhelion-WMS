namespace Parhelion.Domain.Enums;

/// <summary>
/// Permisos granulares del sistema.
/// IMPORTANTE: Estos permisos son INMUTABLES en runtime.
/// Solo pueden modificarse cambiando el código fuente.
/// </summary>
public enum Permission
{
    // ========== USERS ==========
    UsersRead = 100,
    UsersCreate = 101,
    UsersUpdate = 102,
    UsersDelete = 103,
    
    // ========== TRUCKS ==========
    TrucksRead = 200,
    TrucksCreate = 201,
    TrucksUpdate = 202,
    TrucksDelete = 203,
    
    // ========== DRIVERS ==========
    DriversRead = 300,
    DriversCreate = 301,
    DriversUpdate = 302,
    DriversDelete = 303,
    
    // ========== CLIENTS ==========
    ClientsRead = 400,
    ClientsCreate = 401,
    ClientsUpdate = 402,
    ClientsDelete = 403,
    
    // ========== SHIPMENTS ==========
    ShipmentsRead = 500,
    ShipmentsCreate = 501,
    ShipmentsUpdate = 502,
    ShipmentsDelete = 503,
    ShipmentsAssign = 504,
    ShipmentsReadOwn = 510,         // Driver: solo sus envíos
    ShipmentsReadByLocation = 511,  // Warehouse: envíos de su ubicación
    
    // ========== SHIPMENT ITEMS ==========
    ShipmentItemsRead = 600,
    ShipmentItemsCreate = 601,
    ShipmentItemsUpdate = 602,
    
    // ========== CHECKPOINTS ==========
    CheckpointsRead = 700,
    CheckpointsCreate = 701,
    
    // ========== ROUTES ==========
    RoutesRead = 800,
    RoutesCreate = 801,
    RoutesUpdate = 802,
    RoutesDelete = 803,
    
    // ========== LOCATIONS ==========
    LocationsRead = 900,
    LocationsCreate = 901,
    LocationsUpdate = 902,
    LocationsDelete = 903,
    
    // ========== DOCUMENTS ==========
    DocumentsRead = 1000,
    DocumentsCreate = 1001,
    DocumentsReadOwn = 1010,        // Driver: solo sus documentos
    
    // ========== FLEET LOGS ==========
    FleetLogsRead = 1100,
    FleetLogsCreate = 1101
}
