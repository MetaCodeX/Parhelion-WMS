using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

/// <summary>
/// Configuración EF Core para Notification.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        
        builder.HasKey(n => n.Id);
        
        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(2000);
            
        builder.Property(n => n.MetadataJson)
            .HasMaxLength(4000);
            
        builder.Property(n => n.RelatedEntityType)
            .HasMaxLength(100);
            
        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(n => n.Source)
            .HasConversion<string>()
            .HasMaxLength(30);
        
        // Índices
        builder.HasIndex(n => n.TenantId);
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.RoleId);
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => new { n.TenantId, n.UserId, n.IsRead });
        
        // Relaciones
        builder.HasOne(n => n.Tenant)
            .WithMany()
            .HasForeignKey(n => n.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(n => n.Role)
            .WithMany()
            .HasForeignKey(n => n.RoleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
