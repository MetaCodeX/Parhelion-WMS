using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class ShipmentDocumentConfiguration : IEntityTypeConfiguration<ShipmentDocument>
{
    public void Configure(EntityTypeBuilder<ShipmentDocument> builder)
    {
        builder.HasKey(sd => sd.Id);
        
        builder.Property(sd => sd.FileUrl)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(sd => sd.GeneratedBy)
            .IsRequired()
            .HasMaxLength(50);
        
        // Relación
        builder.HasOne(sd => sd.Shipment)
            .WithMany(s => s.Documents)
            .HasForeignKey(sd => sd.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
