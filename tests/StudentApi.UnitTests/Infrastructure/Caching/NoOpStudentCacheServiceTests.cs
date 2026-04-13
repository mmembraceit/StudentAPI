using StudentApi.Application.Students;
using StudentApi.Infrastructure.Caching;

namespace StudentApi.UnitTests.Infrastructure.Caching;

public class NoOpStudentCacheServiceTests
{
    [Fact]
    public async Task GetByIdAsync_AlwaysReturnsNull()
    {
        var sut = new NoOpStudentCacheService();

        var result = await sut.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_AlwaysReturnsNull()
    {
        var sut = new NoOpStudentCacheService();

        var result = await sut.GetAllAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task WriteAndInvalidateOperations_DoNotThrow()
    {
        var sut = new NoOpStudentCacheService();
        var tenantId = Guid.NewGuid();
        var student = new StudentDto(Guid.NewGuid(), tenantId, "Alice", new DateOnly(2001, 1, 1));

        await sut.SetByIdAsync(student);
        await sut.SetAllAsync(tenantId, [student]);
        await sut.InvalidateByIdAsync(student.Id, tenantId);
        await sut.InvalidateAllAsync(tenantId);
    }
}
