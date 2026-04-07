using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentApi.Domain.Entities;

namespace StudentApi.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for <see cref="Student"/>.
/// </summary>
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    /// <summary>
    /// Configures table mapping, constraints, and indexes for <see cref="Student"/>.
    /// </summary>
    /// <param name="builder">Entity type builder instance.</param>
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.TenantId)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.DateOfBirth)
            .HasColumnType("date")
            .IsRequired();

        builder.HasIndex(s => new { s.TenantId, s.Name });
    }
}
