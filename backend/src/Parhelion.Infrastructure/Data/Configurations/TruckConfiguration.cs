using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class TruckConfiguration : IEntityTypeConfiguration<Truck>
{
    public void Configure(EntityTypeBuilder<Truck> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Plate)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(t => t.Model)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(t => t.MaxCapacityKg)
            .HasPrecision(10, 2);
            
        builder.Property(t => t.MaxVolumeM3)
            .HasPrecision(10, 2);

        builder.Property(t => t.LastLatitude)
            .HasPrecision(10, 6);
            
        builder.Property(t => t.LastLongitude)
            .HasPrecision(10, 6);

        // Placa única por tenant
        builder.HasIndex(t => new { t.TenantId, t.Plate }).IsUnique();
        
        // Índice por tipo para filtros de compatibilidad
        builder.HasIndex(t => new { t.TenantId, t.Type });
        
        // Relación con Tenant
        builder.HasOne(t => t.Tenant)
            .WithMany(te => te.Trucks)
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
