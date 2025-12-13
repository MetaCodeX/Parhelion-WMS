using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.CompanyName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(t => t.ContactEmail)
            .IsRequired()
            .HasMaxLength(256);

        // Índice para búsqueda rápida de tenants activos
        builder.HasIndex(t => t.IsActive);
    }
}
