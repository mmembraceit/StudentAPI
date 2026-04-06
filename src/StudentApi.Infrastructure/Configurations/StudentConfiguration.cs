using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentApi.Domain.Entities;

namespace StudentApi.Infrastructure.Configurations;

/// EF Core configuration for the <c>Student</c> entity.
/// Defines database constraints and mapping without cluttering the DbContext.

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{

    /// Configures the table, primary key, required fields, length, and a compound index by tenant and name.
    
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
