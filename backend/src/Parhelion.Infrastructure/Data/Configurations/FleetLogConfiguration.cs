using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class FleetLogConfiguration : IEntityTypeConfiguration<FleetLog>
{
    public void Configure(EntityTypeBuilder<FleetLog> builder)
    {
        builder.HasKey(fl => fl.Id);

        // Índice para historial de cambios por chofer
        builder.HasIndex(fl => new { fl.DriverId, fl.Timestamp });
        
        // Relaciones
        builder.HasOne(fl => fl.Tenant)
            .WithMany(t => t.FleetLogs)
            .HasForeignKey(fl => fl.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(fl => fl.Driver)
            .WithMany(d => d.FleetHistory)
            .HasForeignKey(fl => fl.DriverId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(fl => fl.OldTruck)
            .WithMany(t => t.OldTruckLogs)
            .HasForeignKey(fl => fl.OldTruckId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(fl => fl.NewTruck)
            .WithMany(t => t.NewTruckLogs)
            .HasForeignKey(fl => fl.NewTruckId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(fl => fl.CreatedBy)
            .WithMany(u => u.CreatedFleetLogs)
            .HasForeignKey(fl => fl.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
