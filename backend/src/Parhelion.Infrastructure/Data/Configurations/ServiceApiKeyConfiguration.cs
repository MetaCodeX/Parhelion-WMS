using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

/// <summary>
/// Configuración EF Core para ServiceApiKey.
/// </summary>
public class ServiceApiKeyConfiguration : IEntityTypeConfiguration<ServiceApiKey>
{
    public void Configure(EntityTypeBuilder<ServiceApiKey> builder)
    {
        builder.ToTable("ServiceApiKeys");
        
        builder.HasKey(e => e.Id);
        
        // KeyHash: SHA256 = 64 caracteres hex
        builder.Property(e => e.KeyHash)
            .IsRequired()
            .HasMaxLength(64);
        
        // Índice único para búsqueda rápida por hash
        builder.HasIndex(e => e.KeyHash)
            .IsUnique()
            .HasDatabaseName("IX_ServiceApiKeys_KeyHash");
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Description)
            .HasMaxLength(500);
        
        builder.Property(e => e.Scopes)
            .HasMaxLength(1000);
        
        builder.Property(e => e.LastUsedFromIp)
            .HasMaxLength(45); // IPv6 max length
        
        // FK a Tenant
        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índice compuesto para queries por tenant
        builder.HasIndex(e => new { e.TenantId, e.IsActive })
            .HasDatabaseName("IX_ServiceApiKeys_Tenant_Active");
    }
}
