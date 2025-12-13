using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class WarehouseZoneConfiguration : IEntityTypeConfiguration<WarehouseZone>
{
    public void Configure(EntityTypeBuilder<WarehouseZone> builder)
    {
        builder.HasKey(z => z.Id);
        
        builder.Property(z => z.Code)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(z => z.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        // Índice único: código de zona único por ubicación
        builder.HasIndex(z => new { z.LocationId, z.Code }).IsUnique();
        
        // Relación con Location
        builder.HasOne(z => z.Location)
            .WithMany(l => l.Zones)
            .HasForeignKey(z => z.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
