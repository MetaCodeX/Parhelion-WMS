using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class WarehouseOperatorConfiguration : IEntityTypeConfiguration<WarehouseOperator>
{
    public void Configure(EntityTypeBuilder<WarehouseOperator> builder)
    {
        builder.HasKey(w => w.Id);
        
        // Índice único: Un empleado solo puede tener un perfil de almacenista
        builder.HasIndex(w => w.EmployeeId).IsUnique();
        
        // Relación 1:1 con Employee
        builder.HasOne(w => w.Employee)
            .WithOne(e => e.WarehouseOperator)
            .HasForeignKey<WarehouseOperator>(w => w.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Relación con Location (ubicación asignada)
        builder.HasOne(w => w.AssignedLocation)
            .WithMany(l => l.AssignedWarehouseOperators)
            .HasForeignKey(w => w.AssignedLocationId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Relación con WarehouseZone (opcional)
        builder.HasOne(w => w.PrimaryZone)
            .WithMany(z => z.AssignedOperators)
            .HasForeignKey(w => w.PrimaryZoneId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
