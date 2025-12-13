using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Parhelion.Application.Auth;
using Parhelion.Infrastructure.Auth;
using Parhelion.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========== DATABASE ==========
// Usar connection string de variables de entorno o appsettings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ParhelionDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly("Parhelion.Infrastructure");
        npgsqlOptions.EnableRetryOnFailure(3);
    }));

// ========== AUTH SERVICES ==========
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// ========== JWT AUTHENTICATION ==========
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] 
    ?? "ParhelionLogisticsDefaultSecretKey2024!";

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
    
    // Auto-migrate en desarrollo
    if (app.Environment.IsDevelopment())
    {
        await db.Database.MigrateAsync();
    }
    
    // Seed data siempre (es idempotente)
    await SeedData.InitializeAsync(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DevCors");

// ========== AUTHENTICATION & AUTHORIZATION ==========
app.UseAuthentication();
app.UseAuthorization();

// ========== CONTROLLERS ==========
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new
{
    status = "healthy",
    service = "Parhelion API",
    timestamp = DateTime.UtcNow,
    version = "0.4.2",
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

app.Run();

