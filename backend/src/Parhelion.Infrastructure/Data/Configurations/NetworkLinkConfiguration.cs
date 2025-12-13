using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class NetworkLinkConfiguration : IEntityTypeConfiguration<NetworkLink>
{
    public void Configure(EntityTypeBuilder<NetworkLink> builder)
    {
        builder.HasKey(nl => nl.Id);

        // Índices para búsqueda de rutas
        builder.HasIndex(nl => nl.OriginLocationId);
        builder.HasIndex(nl => nl.DestinationLocationId);
        
        // Relaciones
        builder.HasOne(nl => nl.Tenant)
            .WithMany(t => t.NetworkLinks)
            .HasForeignKey(nl => nl.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(nl => nl.OriginLocation)
            .WithMany(l => l.OutgoingLinks)
            .HasForeignKey(nl => nl.OriginLocationId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(nl => nl.DestinationLocation)
            .WithMany(l => l.IncomingLinks)
            .HasForeignKey(nl => nl.DestinationLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
