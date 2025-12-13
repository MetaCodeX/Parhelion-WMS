using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Phone)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(e => e.Rfc)
            .HasMaxLength(13);
            
        builder.Property(e => e.Nss)
            .HasMaxLength(11);
            
        builder.Property(e => e.Curp)
            .HasMaxLength(18);
            
        builder.Property(e => e.EmergencyContact)
            .HasMaxLength(200);
            
        builder.Property(e => e.EmergencyPhone)
            .HasMaxLength(20);
            
        builder.Property(e => e.Department)
            .HasMaxLength(50);
        
        // Índice único: Un usuario solo puede tener un perfil de empleado
        builder.HasIndex(e => e.UserId).IsUnique();
        
        // Índice por tenant para listados
        builder.HasIndex(e => new { e.TenantId, e.Department });
        
        // Relación con Tenant
        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Relación 1:1 con User
        builder.HasOne(e => e.User)
            .WithOne(u => u.Employee)
            .HasForeignKey<Employee>(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Relación con Shift (opcional)
        builder.HasOne(e => e.Shift)
            .WithMany(s => s.Employees)
            .HasForeignKey(e => e.ShiftId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
