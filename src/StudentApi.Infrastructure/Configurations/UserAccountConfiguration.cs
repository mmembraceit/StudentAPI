using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentApi.Domain.Entities;

namespace StudentApi.Infrastructure.Configurations;

public class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
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