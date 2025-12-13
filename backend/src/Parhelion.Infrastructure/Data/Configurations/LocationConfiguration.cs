using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.HasKey(l => l.Id);
        
        builder.Property(l => l.Code)
            .IsRequired()
            .HasMaxLength(10);
            
        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(l => l.FullAddress)
            .IsRequired()
            .HasMaxLength(500);

        // Código único por tenant (estilo aeropuerto: MTY, GDL, MM)
        builder.HasIndex(l => new { l.TenantId, l.Code }).IsUnique();
        
        // Índice por tipo para filtros
        builder.HasIndex(l => new { l.TenantId, l.Type });
        
        // Relación con Tenant
        builder.HasOne(l => l.Tenant)
            .WithMany(t => t.Locations)
            .HasForeignKey(l => l.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
