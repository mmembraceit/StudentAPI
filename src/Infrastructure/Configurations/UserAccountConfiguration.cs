using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentApi.Domain.Entities;

namespace StudentApi.Infrastructure.Configurations;


/// EF Core configuration for <see cref="UserAccount"/>, including seeded admin user.
public class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
  
    /// Configures table mapping, constraints, indexes, and development seed data.
    /// <param name="builder">Entity type builder instance.</param>
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("UserAccounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => x.Username)
            .IsUnique();
        
        // Seed an initial admin user (DATA SEEDING)
        builder.HasData(new UserAccount
        {
            Id = Guid.Parse("9f0e6c26-0ff2-4bb9-93dd-f2bf5074a9a3"),
            Username = "admin",
            PasswordHash = "100000.YgFY3Gm4EwL1lz+uDGx69g==.w8AMSr3pbnGTpY5ZDgOD+9gmwWknPiOYO4q512LezBE=",
            Role = "Admin",
            IsActive = true
        });
    }
}