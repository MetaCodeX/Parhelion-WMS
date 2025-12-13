using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class ShipmentItemConfiguration : IEntityTypeConfiguration<ShipmentItem>
{
    public void Configure(EntityTypeBuilder<ShipmentItem> builder)
    {
        builder.HasKey(si => si.Id);
        
        builder.Property(si => si.Sku)
            .HasMaxLength(50);
            
        builder.Property(si => si.Description)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(si => si.WeightKg)
            .HasPrecision(10, 2);
            
        builder.Property(si => si.WidthCm)
            .HasPrecision(10, 2);
            
        builder.Property(si => si.HeightCm)
            .HasPrecision(10, 2);
            
        builder.Property(si => si.LengthCm)
            .HasPrecision(10, 2);
            
        builder.Property(si => si.DeclaredValue)
            .HasPrecision(18, 2);
            
        builder.Property(si => si.StackingInstructions)
            .HasMaxLength(500);

        // VolumeM3 es una propiedad calculada, no se mapea a columna
        builder.Ignore(si => si.VolumeM3);
        builder.Ignore(si => si.VolumetricWeightKg);
        
        // Relación
        builder.HasOne(si => si.Shipment)
            .WithMany(s => s.Items)
            .HasForeignKey(si => si.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
