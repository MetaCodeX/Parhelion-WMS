using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.LicenseNumber)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(d => d.LicenseType)
            .HasMaxLength(10);
        
        // Índice único: Un empleado solo puede tener un perfil de driver
        builder.HasIndex(d => d.EmployeeId).IsUnique();
        
        // Índice por status para dashboard
        builder.HasIndex(d => d.Status);
        
        // Relación 1:1 con Employee
        builder.HasOne(d => d.Employee)
            .WithOne(e => e.Driver)
            .HasForeignKey<Driver>(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(d => d.DefaultTruck)
            .WithMany(t => t.DefaultDrivers)
            .HasForeignKey(d => d.DefaultTruckId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(d => d.CurrentTruck)
            .WithMany(t => t.CurrentDrivers)
            .HasForeignKey(d => d.CurrentTruckId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
