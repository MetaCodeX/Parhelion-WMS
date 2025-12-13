using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class RouteBlueprintConfiguration : IEntityTypeConfiguration<RouteBlueprint>
{
    public void Configure(EntityTypeBuilder<RouteBlueprint> builder)
    {
        builder.HasKey(rb => rb.Id);
        
        builder.Property(rb => rb.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(rb => rb.Description)
            .HasMaxLength(500);
        
        // Relación
        builder.HasOne(rb => rb.Tenant)
            .WithMany(t => t.RouteBlueprints)
            .HasForeignKey(rb => rb.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
