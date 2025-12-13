using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.TrackingNumber)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(s => s.QrCodeData)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(s => s.RecipientName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(s => s.RecipientPhone)
            .HasMaxLength(20);
            
        builder.Property(s => s.TotalWeightKg)
            .HasPrecision(10, 2);
            
        builder.Property(s => s.TotalVolumeM3)
            .HasPrecision(10, 3);
            
        builder.Property(s => s.DeclaredValue)
            .HasPrecision(18, 2);
            
        builder.Property(s => s.SatMerchandiseCode)
            .HasMaxLength(20);
            
        builder.Property(s => s.DeliveryInstructions)
            .HasMaxLength(1000);
            
        builder.Property(s => s.RecipientSignatureUrl)
            .HasMaxLength(500);

        // SEGURIDAD: Tracking number único global
        builder.HasIndex(s => s.TrackingNumber).IsUnique();
        
        // Índices para dashboard y filtros frecuentes
        builder.HasIndex(s => new { s.TenantId, s.Status });
        builder.HasIndex(s => new { s.TenantId, s.CreatedAt });
        builder.HasIndex(s => new { s.TenantId, s.IsDelayed })
            .HasFilter("\"IsDelayed\" = true");
        builder.HasIndex(s => s.DriverId);
        builder.HasIndex(s => s.OriginLocationId);
        builder.HasIndex(s => s.DestinationLocationId);
        
        // Relaciones
        builder.HasOne(s => s.Tenant)
            .WithMany(t => t.Shipments)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(s => s.OriginLocation)
            .WithMany(l => l.OriginShipments)
            .HasForeignKey(s => s.OriginLocationId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(s => s.DestinationLocation)
            .WithMany(l => l.DestinationShipments)
            .HasForeignKey(s => s.DestinationLocationId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(s => s.AssignedRoute)
            .WithMany(r => r.Shipments)
            .HasForeignKey(s => s.AssignedRouteId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(s => s.Truck)
            .WithMany(t => t.Shipments)
            .HasForeignKey(s => s.TruckId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(s => s.Driver)
            .WithMany(d => d.Shipments)
            .HasForeignKey(s => s.DriverId)
            .OnDelete(DeleteBehavior.SetNull);
            
        // Relación con Client (remitente)
        builder.HasOne(s => s.Sender)
            .WithMany(c => c.ShipmentsAsSender)
            .HasForeignKey(s => s.SenderId)
            .OnDelete(DeleteBehavior.SetNull);
            
        // Relación con Client (destinatario)
        builder.HasOne(s => s.RecipientClient)
            .WithMany(c => c.ShipmentsAsRecipient)
            .HasForeignKey(s => s.RecipientClientId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
