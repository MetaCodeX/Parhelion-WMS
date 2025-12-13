using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");
        
        builder.HasKey(c => c.Id);
        
        // Basic data
        builder.Property(c => c.CompanyName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(c => c.TradeName)
            .HasMaxLength(200);
            
        builder.Property(c => c.ContactName)
            .IsRequired()
            .HasMaxLength(150);
            
        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(150);
            
        builder.Property(c => c.Phone)
            .IsRequired()
            .HasMaxLength(30);
        
        // Fiscal data
        builder.Property(c => c.TaxId)
            .HasMaxLength(20); // RFC mexicano
            
        builder.Property(c => c.LegalName)
            .HasMaxLength(300);
            
        builder.Property(c => c.BillingAddress)
            .HasMaxLength(500);
        
        // Shipping data
        builder.Property(c => c.ShippingAddress)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(c => c.PreferredProductTypes)
            .HasMaxLength(300);
            
        builder.Property(c => c.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(c => c.Notes)
            .HasMaxLength(1000);
        
        // Soft Delete Query Filter
        builder.HasQueryFilter(c => !c.IsDeleted);
        
        // Indexes
        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => c.Email);
        builder.HasIndex(c => new { c.TenantId, c.CompanyName });
        
        // Relationships
        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
