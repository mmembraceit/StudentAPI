using Microsoft.EntityFrameworkCore;
using StudentApi.Domain.Entities;

namespace StudentApi.Infrastructure.Persistence;


/// Main EF Core DbContext for the application.
/// Is in Infrastructure because it knows persistence details.
/// It is related to <c>StudentConfiguration</c>, migrations, and <c>StudentRepository</c>.

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();

  
    /// Automatically applies all entity configurations from the assembly.
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
