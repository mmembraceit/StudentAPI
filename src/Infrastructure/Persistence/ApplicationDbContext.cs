using Microsoft.EntityFrameworkCore;
using StudentApi.Domain.Entities;

namespace StudentApi.Infrastructure.Persistence;


/// Main EF Core DbContext for application persistence.
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// Student entity set.
    public DbSet<Student> Students => Set<Student>();

    /// User account entity set.
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    /// Refresh-token entity set.
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// Applies entity configurations from the assembly.
    /// <param name="modelBuilder">Model builder used to configure entities.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
