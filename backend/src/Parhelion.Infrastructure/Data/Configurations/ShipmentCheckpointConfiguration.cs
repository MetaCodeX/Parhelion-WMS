using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class ShipmentCheckpointConfiguration : IEntityTypeConfiguration<ShipmentCheckpoint>
{
    public void Configure(EntityTypeBuilder<ShipmentCheckpoint> builder)
    {
        builder.HasKey(sc => sc.Id);
        
        builder.Property(sc => sc.Remarks)
            .HasMaxLength(1000);
            
        // Campos de trazabilidad de cargueros
        builder.Property(sc => sc.ActionType)
            .HasMaxLength(50);
            
        builder.Property(sc => sc.PreviousCustodian)
            .HasMaxLength(200);
            
        builder.Property(sc => sc.NewCustodian)
            .HasMaxLength(200);

        // Índices para trazabilidad
        builder.HasIndex(sc => sc.ShipmentId);
        builder.HasIndex(sc => sc.Timestamp);
        builder.HasIndex(sc => sc.HandledByDriverId);
        builder.HasIndex(sc => sc.LoadedOntoTruckId);
        
        // Relaciones
        builder.HasOne(sc => sc.Shipment)
            .WithMany(s => s.History)
            .HasForeignKey(sc => sc.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(sc => sc.Location)
            .WithMany(l => l.Checkpoints)
            .HasForeignKey(sc => sc.LocationId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(sc => sc.CreatedBy)
            .WithMany(u => u.CreatedCheckpoints)
            .HasForeignKey(sc => sc.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Trazabilidad de cargueros
        builder.HasOne(sc => sc.HandledByDriver)
            .WithMany(d => d.HandledCheckpoints)
            .HasForeignKey(sc => sc.HandledByDriverId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(sc => sc.LoadedOntoTruck)
            .WithMany(t => t.LoadedCheckpoints)
            .HasForeignKey(sc => sc.LoadedOntoTruckId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
