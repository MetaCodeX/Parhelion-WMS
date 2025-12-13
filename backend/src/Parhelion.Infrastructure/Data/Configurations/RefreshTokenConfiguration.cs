using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        
        builder.HasKey(rt => rt.Id);
        
        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();
            
        builder.Property(rt => rt.IsRevoked)
            .HasDefaultValue(false);
            
        builder.Property(rt => rt.RevokedReason)
            .HasMaxLength(200);
            
        builder.Property(rt => rt.CreatedFromIp)
            .HasMaxLength(45); // IPv6 max length
            
        builder.Property(rt => rt.UserAgent)
            .HasMaxLength(500);
        
        // Indexes
        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => rt.TokenHash);
        builder.HasIndex(rt => rt.ExpiresAt);
        
        // Relationships
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
