using Microsoft.EntityFrameworkCore;
using StudentApi.Domain.Entities;

namespace StudentApi.Infrastructure.Persistence;

/// <summary>
/// Main EF Core DbContext for application persistence.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Student entity set.
    /// </summary>
    public DbSet<Student> Students => Set<Student>();

    /// <summary>
    /// User account entity set.
    /// </summary>
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    /// <summary>
    /// Refresh-token entity set.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// Applies entity configurations from the assembly.
    /// </summary>
    /// <param name="modelBuilder">Model builder used to configure entities.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
