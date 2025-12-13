using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class RouteStepConfiguration : IEntityTypeConfiguration<RouteStep>
{
    public void Configure(EntityTypeBuilder<RouteStep> builder)
    {
        builder.HasKey(rs => rs.Id);

        // Índice para ordenar pasos de ruta
        builder.HasIndex(rs => new { rs.RouteBlueprintId, rs.StepOrder });
        
        // Relaciones
        builder.HasOne(rs => rs.RouteBlueprint)
            .WithMany(rb => rb.Steps)
            .HasForeignKey(rs => rs.RouteBlueprintId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(rs => rs.Location)
            .WithMany(l => l.RouteSteps)
            .HasForeignKey(rs => rs.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
