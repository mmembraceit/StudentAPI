using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StudentApi.Domain.Entities;
using StudentApi.Infrastructure.Persistence;
using StudentApi.Infrastructure.Repositories;

namespace StudentApi.IntegrationTests;

public class StudentRepositoryIntegrationTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOnlyStudentsFromRequestedTenant()
    {
        await using var fixture = await SqliteFixture.CreateAsync();
        var repository = new StudentRepository(fixture.DbContext);

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        fixture.DbContext.Students.AddRange(
            new Student { Id = Guid.NewGuid(), TenantId = tenantA, Name = "A-1", DateOfBirth = new DateOnly(2001, 1, 1) },
            new Student { Id = Guid.NewGuid(), TenantId = tenantA, Name = "A-2", DateOfBirth = new DateOnly(2001, 2, 2) },
            new Student { Id = Guid.NewGuid(), TenantId = tenantB, Name = "B-1", DateOfBirth = new DateOnly(2001, 3, 3) });
        await fixture.DbContext.SaveChangesAsync();

        var tenantAStudents = await repository.GetAllAsync(tenantA);

        Assert.Equal(2, tenantAStudents.Count);
        Assert.All(tenantAStudents, s => Assert.Equal(tenantA, s.TenantId));
    }

    [Fact]
    public async Task DeleteAsync_DeletesOnlyWithinTenantScope()
    {
        await using var fixture = await SqliteFixture.CreateAsync();
        var repository = new StudentRepository(fixture.DbContext);

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var id = Guid.NewGuid();

        fixture.DbContext.Students.AddRange(
            new Student { Id = id, TenantId = tenantA, Name = "A", DateOfBirth = new DateOnly(2001, 1, 1) },
            new Student { Id = Guid.NewGuid(), TenantId = tenantB, Name = "B", DateOfBirth = new DateOnly(2001, 2, 2) });
        await fixture.DbContext.SaveChangesAsync();

        await repository.DeleteAsync(id, tenantB);

        var stillExists = await fixture.DbContext.Students.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantA);
        Assert.NotNull(stillExists);

        await repository.DeleteAsync(id, tenantA);

        var deleted = await fixture.DbContext.Students.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantA);
        Assert.Null(deleted);
    }

    private sealed class SqliteFixture : IAsyncDisposable
    {
        public SqliteConnection Connection { get; }
        public ApplicationDbContext DbContext { get; }

        private SqliteFixture(SqliteConnection connection, ApplicationDbContext dbContext)
        {
            Connection = connection;
            DbContext = dbContext;
        }

        public static async Task<SqliteFixture> CreateAsync()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new ApplicationDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            return new SqliteFixture(connection, dbContext);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
}
