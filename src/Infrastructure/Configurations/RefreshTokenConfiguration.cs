using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentApi.Domain.Entities;

namespace StudentApi.Infrastructure.Configurations;

/// EF Core configuration for <see cref="RefreshToken"/>.
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
  
    /// Configures table mapping, constraints, and indexes for refresh tokens.
    /// <param name="builder">Entity type builder instance.</param>
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserAccountId)
            .IsRequired();

        builder.Property(x => x.Username)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        builder.Property(x => x.RevokedAtUtc)
            .IsRequired(false);

        builder.Property(x => x.ReplacedByTokenHash)
            .HasMaxLength(128)
            .IsRequired(false);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasIndex(x => new { x.UserAccountId, x.ExpiresAtUtc });
    }
}
