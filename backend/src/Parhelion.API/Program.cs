using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Parhelion.Application.Auth;
using Parhelion.Application.Services;
using Parhelion.Infrastructure.Auth;
using Parhelion.Infrastructure.Data;
using Parhelion.Infrastructure.Data.Interceptors;
using Parhelion.Infrastructure.Services;
using Parhelion.Infrastructure.External.Webhooks;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ========== SWAGGER/OPENAPI CONFIGURATION ==========
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v0.5.7",
        Title = "Parhelion Logistics API",
        Description = "API para gestión de logística B2B: envíos, flotas, rutas y almacenes (WMS + TMS)",
        Contact = new OpenApiContact
        {
            Name = "Parhelion Logistics",
            Email = "dev@parhelion.com"
        },
        License = new OpenApiLicense
        {
            Name = "Proprietary",
            Url = new Uri("https://parhelion.com/license")
        }
    });

    // JWT Bearer Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Formato: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML Comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// ========== INFRASTRUCTURE SERVICES ==========
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<AuditSaveChangesInterceptor>();

// ========== DATABASE ==========
// Connection string desde variable de entorno o appsettings
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "parhelion_dev";
var connectionString = !string.IsNullOrEmpty(builder.Configuration.GetConnectionString("DefaultConnection"))
    ? builder.Configuration.GetConnectionString("DefaultConnection")!
    : $"Host={dbHost};Port=5432;Database={dbName};Username={dbUser};Password={dbPassword}";

builder.Services.AddDbContext<ParhelionDbContext>((sp, options) =>
{
    var auditInterceptor = sp.GetRequiredService<AuditSaveChangesInterceptor>();
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly("Parhelion.Infrastructure");
        npgsqlOptions.EnableRetryOnFailure(3);
    })
    .AddInterceptors(auditInterceptor);
});

// ========== AUTH SERVICES ==========
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// ========== REPOSITORY PATTERN ==========
builder.Services.AddScoped<Parhelion.Application.Interfaces.IUnitOfWork, 
    Parhelion.Infrastructure.Repositories.UnitOfWork>();

// ========== CORE LAYER SERVICES ==========
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.ITenantService, 
    Parhelion.Infrastructure.Services.Core.TenantService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IUserService, 
    Parhelion.Infrastructure.Services.Core.UserService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IRoleService, 
    Parhelion.Infrastructure.Services.Core.RoleService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IEmployeeService, 
    Parhelion.Infrastructure.Services.Core.EmployeeService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IClientService, 
    Parhelion.Infrastructure.Services.Core.ClientService>();

// ========== SHIPMENT LAYER SERVICES ==========
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IShipmentService, 
    Parhelion.Infrastructure.Services.Shipment.ShipmentService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IShipmentItemService, 
    Parhelion.Infrastructure.Services.Shipment.ShipmentItemService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IShipmentCheckpointService, 
    Parhelion.Infrastructure.Services.Shipment.ShipmentCheckpointService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IShipmentDocumentService, 
    Parhelion.Infrastructure.Services.Shipment.ShipmentDocumentService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.ICatalogItemService, 
    Parhelion.Infrastructure.Services.Shipment.CatalogItemService>();

// ========== FLEET LAYER SERVICES ==========
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IDriverService, 
    Parhelion.Infrastructure.Services.Fleet.DriverService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.ITruckService, 
    Parhelion.Infrastructure.Services.Fleet.TruckService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IFleetLogService, 
    Parhelion.Infrastructure.Services.Fleet.FleetLogService>();

// ========== NETWORK LAYER SERVICES ==========
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.ILocationService, 
    Parhelion.Infrastructure.Services.Network.LocationService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IRouteService, 
    Parhelion.Infrastructure.Services.Network.RouteService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.INetworkLinkService, 
    Parhelion.Infrastructure.Services.Network.NetworkLinkService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IRouteStepService, 
    Parhelion.Infrastructure.Services.Network.RouteStepService>();

// ========== WAREHOUSE LAYER SERVICES ==========
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IWarehouseZoneService, 
    Parhelion.Infrastructure.Services.Warehouse.WarehouseZoneService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IWarehouseOperatorService, 
    Parhelion.Infrastructure.Services.Warehouse.WarehouseOperatorService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IInventoryStockService, 
    Parhelion.Infrastructure.Services.Warehouse.InventoryStockService>();
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.IInventoryTransactionService, 
    Parhelion.Infrastructure.Services.Warehouse.InventoryTransactionService>();

// ========== NOTIFICATION SERVICES ==========
builder.Services.AddScoped<Parhelion.Application.Interfaces.Services.INotificationService, 
    Parhelion.Infrastructure.Services.Notification.NotificationService>();

// ========== PDF GENERATOR SERVICE ==========
builder.Services.AddScoped<Parhelion.Application.Interfaces.IPdfGeneratorService, 
    Parhelion.Infrastructure.Services.Documents.PdfGeneratorService>();


// ========== VALIDATORS ==========
builder.Services.AddSingleton<Parhelion.Application.Interfaces.Validators.ICargoCompatibilityValidator, 
    Parhelion.Infrastructure.Validators.CargoCompatibilityValidator>();

// ========== WEBHOOK/N8N INTEGRATION ==========
// Credenciales se leen de variables de entorno por seguridad
builder.Services.Configure<N8nConfiguration>(options =>
{
    builder.Configuration.GetSection("N8n").Bind(options);
    // Override desde variables de entorno si existen
    var envBaseUrl = Environment.GetEnvironmentVariable("N8N_BASE_URL");
    if (!string.IsNullOrEmpty(envBaseUrl))
    {
        options.BaseUrl = envBaseUrl;
    }
    var envApiKey = Environment.GetEnvironmentVariable("N8N_WEBHOOK_SECRET");
    if (!string.IsNullOrEmpty(envApiKey))
    {
        options.ApiKey = envApiKey;
    }
});

// CallbackTokenService (necesario tanto para publisher como para ServiceApiKeyAttribute)
builder.Services.AddSingleton<Parhelion.Application.Interfaces.ICallbackTokenService, 
    Parhelion.Infrastructure.Services.Auth.CallbackTokenService>();

var n8nEnabled = builder.Configuration.GetValue<bool>("N8n:Enabled");
if (n8nEnabled)
{
    // Si n8n está habilitado, usar el publisher real con HttpClient
    builder.Services.AddHttpClient<Parhelion.Application.Interfaces.IWebhookPublisher, N8nWebhookPublisher>();
}
else
{
    // Si está deshabilitado, usar NullPublisher (no hace nada)
    builder.Services.AddSingleton<Parhelion.Application.Interfaces.IWebhookPublisher, 
        Parhelion.Application.Interfaces.NullWebhookPublisher>();
}

// ========== PYTHON ANALYTICS CLIENT (with Polly resilience) ==========
var pythonAnalyticsUrl = Environment.GetEnvironmentVariable("PYTHON_ANALYTICS_URL") 
    ?? "http://parhelion-python:8000";

builder.Services.AddHttpClient<Parhelion.Application.Interfaces.IPythonAnalyticsClient, 
    Parhelion.Infrastructure.External.PythonAnalyticsClient>(client =>
{
    client.BaseAddress = new Uri(pythonAnalyticsUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("X-Internal-Call", "true");
})
.AddTransientHttpErrorPolicy(policy => 
    policy.WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromMilliseconds(500 * Math.Pow(2, retryAttempt))));


// ========== JWT AUTHENTICATION ==========
// JWT Secret desde variable de entorno
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET") 
    ?? builder.Configuration["Jwt:SecretKey"] 
    ?? throw new InvalidOperationException("JWT_SECRET environment variable or Jwt:SecretKey config is required");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Parhelion",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ParhelionClient",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// CORS para desarrollo
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ========== DATABASE MIGRATION & SEED ==========
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ParhelionDbContext>();
    
    // Auto-migrate - seguro en dev/staging
    // En producción real, usar: dotnet ef database update
    var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        Console.WriteLine($"Applying {pendingMigrations.Count()} pending migration(s)...");
        await db.Database.MigrateAsync();
        Console.WriteLine("Migrations applied successfully.");
    }
    
    // Seed data siempre (es idempotente)
    await SeedData.InitializeAsync(db);
}

// Configure the HTTP request pipeline.
// Swagger habilitado en todos los entornos para acceso via Tailscale
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Parhelion API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors("DevCors");

// ========== AUTHENTICATION & AUTHORIZATION ==========
app.UseAuthentication();
app.UseAuthorization();

// ========== STATIC FILES (for uploads) ==========
app.UseStaticFiles(); // Sirve archivos desde wwwroot y /uploads

// ========== CONTROLLERS ==========
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new
{
    status = "healthy",
    service = "Parhelion API",
    timestamp = DateTime.UtcNow,
    version = "0.5.7",
    database = "PostgreSQL"
})
.WithName("HealthCheck")
.WithOpenApi();

// Database status endpoint
app.MapGet("/health/db", async (ParhelionDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
        
        return Results.Ok(new
        {
            status = canConnect ? "connected" : "disconnected",
            pendingMigrations = pendingMigrations.Count(),
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("DatabaseHealthCheck")
.WithOpenApi();

// Seed initial data (SuperUser, DefaultTenant) if database is empty
await Parhelion.API.Data.DataSeeder.SeedAsync(app.Services);

// Seed Crisis Management scenarios (for dev/testing)
// await Parhelion.API.Data.CrisisSeeder.SeedAsync(app.Services);

app.Run();
