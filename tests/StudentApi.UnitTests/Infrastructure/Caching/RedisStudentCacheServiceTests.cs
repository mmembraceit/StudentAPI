using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StudentApi.Application.Students;
using StudentApi.Infrastructure.Caching;

namespace StudentApi.UnitTests.Infrastructure.Caching;

public class RedisStudentCacheServiceTests
{
    [Fact]
    public async Task SetByIdAsync_ThenGetByIdAsync_ReturnsStudent()
    {
        var sut = CreateSut();
        var tenantId = Guid.NewGuid();
        var student = new StudentDto(Guid.NewGuid(), tenantId, "Alice", new DateOnly(2001, 1, 1));

        await sut.SetByIdAsync(student);
        var cachedStudent = await sut.GetByIdAsync(student.Id, tenantId);

        Assert.Equal(student, cachedStudent);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        var sut = CreateSut();

        var cachedStudent = await sut.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(cachedStudent);
    }

    [Fact]
    public async Task SetAllAsync_ThenGetAllAsync_ReturnsStudents()
    {
        var sut = CreateSut();
        var tenantId = Guid.NewGuid();

        var students = new List<StudentDto>
        {
            new(Guid.NewGuid(), tenantId, "Alice", new DateOnly(2001, 1, 1)),
            new(Guid.NewGuid(), tenantId, "Bob", new DateOnly(2002, 2, 2))
        };

        await sut.SetAllAsync(tenantId, students);
        var cachedStudents = await sut.GetAllAsync(tenantId);

        Assert.NotNull(cachedStudents);
        Assert.Equal(students, cachedStudents);
    }

    [Fact]
    public async Task InvalidateByIdAsync_RemovesStudentEntry()
    {
        var sut = CreateSut();
        var tenantId = Guid.NewGuid();
        var student = new StudentDto(Guid.NewGuid(), tenantId, "Alice", new DateOnly(2001, 1, 1));

        await sut.SetByIdAsync(student);
        await sut.InvalidateByIdAsync(student.Id, tenantId);

        var cachedStudent = await sut.GetByIdAsync(student.Id, tenantId);
        Assert.Null(cachedStudent);
    }

    [Fact]
    public async Task InvalidateAllAsync_RemovesStudentsEntry()
    {
        var sut = CreateSut();
        var tenantId = Guid.NewGuid();
        IReadOnlyList<StudentDto> students =
        [
            new StudentDto(Guid.NewGuid(), tenantId, "Alice", new DateOnly(2001, 1, 1))
        ];

        await sut.SetAllAsync(tenantId, students);
        await sut.InvalidateAllAsync(tenantId);

        var cachedStudents = await sut.GetAllAsync(tenantId);
        Assert.Null(cachedStudents);
    }

    private static RedisStudentCacheService CreateSut()
    {
        var distributedCache = CreateDistributedCache();
        return new RedisStudentCacheService(distributedCache, NullLogger<RedisStudentCacheService>.Instance);
    }

    private static IDistributedCache CreateDistributedCache()
    {
        var options = Options.Create(new MemoryDistributedCacheOptions());
        return new MemoryDistributedCache(options);
    }
}
