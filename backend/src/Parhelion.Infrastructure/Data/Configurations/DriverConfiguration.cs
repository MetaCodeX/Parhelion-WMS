using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.FullName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(d => d.Phone)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(d => d.LicenseNumber)
            .IsRequired()
            .HasMaxLength(50);

        // Índice por tenant y status para dashboard
        builder.HasIndex(d => new { d.TenantId, d.Status });
        
        // Relaciones
        builder.HasOne(d => d.Tenant)
            .WithMany(t => t.Drivers)
            .HasForeignKey(d => d.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(d => d.User)
            .WithOne(u => u.Driver)
            .HasForeignKey<Driver>(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
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
