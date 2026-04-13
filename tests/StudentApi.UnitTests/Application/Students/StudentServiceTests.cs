using StudentApi.Application.Common.Exceptions;
using StudentApi.Application.Interfaces;
using StudentApi.Application.Students;
using StudentApi.Application.Students.Events;
using StudentApi.Domain.Entities;

namespace StudentApi.UnitTests.Application.Students;

public class StudentServiceTests
{
    [Fact]
    public async Task GetByIdAsync_WhenCacheHit_ReturnsCachedStudent_WithoutRepositoryCall()
    {
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var cachedStudent = new StudentDto(studentId, tenantId, "Cached", new DateOnly(2001, 1, 1));

        var repository = new FakeStudentRepository();
        var cache = new FakeStudentCacheService { GetByIdResult = cachedStudent };
        var publisher = new FakeStudentEventPublisher();
        var sut = new StudentService(repository, cache, publisher);

        var result = await sut.GetByIdAsync(studentId, tenantId);

        Assert.Equal(cachedStudent, result);
        Assert.Equal(0, repository.GetByIdCalls);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCacheMissAndFound_SetsCacheAndReturnsStudent()
    {
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var student = new Student { Id = studentId, TenantId = tenantId, Name = "Alice", DateOfBirth = new DateOnly(2001, 2, 2) };

        var repository = new FakeStudentRepository { GetByIdResult = student };
        var cache = new FakeStudentCacheService();
        var publisher = new FakeStudentEventPublisher();
        var sut = new StudentService(repository, cache, publisher);

        var result = await sut.GetByIdAsync(studentId, tenantId);

        Assert.Equal(student.Id, result.Id);
        Assert.Equal(student.Name, result.Name);
        Assert.Equal(1, repository.GetByIdCalls);
        Assert.Equal(1, cache.SetByIdCalls);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        var repository = new FakeStudentRepository();
        var cache = new FakeStudentCacheService();
        var publisher = new FakeStudentEventPublisher();
        var sut = new StudentService(repository, cache, publisher);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllAsync_WhenCacheHit_ReturnsCachedStudents_WithoutRepositoryCall()
    {
        var tenantId = Guid.NewGuid();
        IReadOnlyList<StudentDto> cached =
        [
            new(Guid.NewGuid(), tenantId, "Alice", new DateOnly(2001, 1, 1))
        ];

        var repository = new FakeStudentRepository();
        var cache = new FakeStudentCacheService { GetAllResult = cached };
        var publisher = new FakeStudentEventPublisher();
        var sut = new StudentService(repository, cache, publisher);

        var result = await sut.GetAllAsync(tenantId);

        Assert.Equal(cached, result);
        Assert.Equal(0, repository.GetAllCalls);
    }

    [Fact]
    public async Task CreateAsync_PersistsAndUpdatesCacheAndPublishesCreatedEvent()
    {
        var tenantId = Guid.NewGuid();
        var request = new CreateStudentRequest(tenantId, "Alice", new DateOnly(2001, 1, 1));

        var repository = new FakeStudentRepository();
        var cache = new FakeStudentCacheService();
        var publisher = new FakeStudentEventPublisher();
        var sut = new StudentService(repository, cache, publisher);

        var result = await sut.CreateAsync(request);

        Assert.Equal(1, repository.AddCalls);
        Assert.Equal(1, cache.SetByIdCalls);
        Assert.Equal(1, cache.InvalidateAllCalls);
        var createdEvent = Assert.Single(publisher.CreatedEvents);
        Assert.Equal(result.Id, createdEvent.StudentId);
    }

    [Fact]
    public async Task UpdateAsync_WhenFound_UpdatesAndPublishesUpdatedEvent()
    {
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var current = new Student { Id = studentId, TenantId = tenantId, Name = "Old", DateOfBirth = new DateOnly(2000, 1, 1) };

        var repository = new FakeStudentRepository { GetByIdResult = current };
        var cache = new FakeStudentCacheService();
        var publisher = new FakeStudentEventPublisher();
        var sut = new StudentService(repository, cache, publisher);

        var result = await sut.UpdateAsync(studentId, tenantId, new UpdateStudentRequest("New", new DateOnly(2002, 2, 2)));

        Assert.Equal("New", result.Name);
        Assert.Equal(1, repository.UpdateCalls);
        Assert.Equal(1, cache.SetByIdCalls);
        Assert.Equal(1, cache.InvalidateAllCalls);
        var updatedEvent = Assert.Single(publisher.UpdatedEvents);
        Assert.Equal(studentId, updatedEvent.StudentId);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ThrowsNotFoundException()
    {
        var repository = new FakeStudentRepository();
        var cache = new FakeStudentCacheService();
        var publisher = new FakeStudentEventPublisher();
        var sut = new StudentService(repository, cache, publisher);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateStudentRequest("Name", new DateOnly(2001, 1, 1))));
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_DeletesInvalidatesAndPublishesDeletedEvent()
    {
        var tenantId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var current = new Student { Id = studentId, TenantId = tenantId, Name = "Alice", DateOfBirth = new DateOnly(2001, 1, 1) };

        var repository = new FakeStudentRepository { GetByIdResult = current };
        var cache = new FakeStudentCacheService();
        var publisher = new FakeStudentEventPublisher();
        var sut = new StudentService(repository, cache, publisher);

        await sut.DeleteAsync(new DeleteStudentRequest(studentId, tenantId));

        Assert.Equal(1, repository.DeleteCalls);
        Assert.Equal(1, cache.InvalidateByIdCalls);
        Assert.Equal(1, cache.InvalidateAllCalls);
        var deletedEvent = Assert.Single(publisher.DeletedEvents);
        Assert.Equal(studentId, deletedEvent.StudentId);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ThrowsNotFoundException()
    {
        var repository = new FakeStudentRepository();
        var cache = new FakeStudentCacheService();
        var publisher = new FakeStudentEventPublisher();
        var sut = new StudentService(repository, cache, publisher);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.DeleteAsync(new DeleteStudentRequest(Guid.NewGuid(), Guid.NewGuid())));
    }

    private sealed class FakeStudentRepository : IStudentRepository
    {
        public Student? GetByIdResult { get; set; }
        public IReadOnlyList<Student> GetAllResult { get; set; } = [];

        public int GetByIdCalls { get; private set; }
        public int GetAllCalls { get; private set; }
        public int AddCalls { get; private set; }
        public int UpdateCalls { get; private set; }
        public int DeleteCalls { get; private set; }

        public Task<Student?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
        {
            GetByIdCalls++;
            return Task.FromResult(GetByIdResult);
        }

        public Task<IReadOnlyList<Student>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            GetAllCalls++;
            return Task.FromResult(GetAllResult);
        }

        public Task AddAsync(Student student, CancellationToken cancellationToken = default)
        {
            AddCalls++;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
        {
            UpdateCalls++;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
        {
            DeleteCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeStudentCacheService : IStudentCacheService
    {
        public StudentDto? GetByIdResult { get; set; }
        public IReadOnlyList<StudentDto>? GetAllResult { get; set; }

        public int SetByIdCalls { get; private set; }
        public int SetAllCalls { get; private set; }
        public int InvalidateByIdCalls { get; private set; }
        public int InvalidateAllCalls { get; private set; }

        public Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult(GetByIdResult);

        public Task SetByIdAsync(StudentDto student, CancellationToken cancellationToken = default)
        {
            SetByIdCalls++;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<StudentDto>?> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult(GetAllResult);

        public Task SetAllAsync(Guid tenantId, IReadOnlyList<StudentDto> students, CancellationToken cancellationToken = default)
        {
            SetAllCalls++;
            return Task.CompletedTask;
        }

        public Task InvalidateByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
        {
            InvalidateByIdCalls++;
            return Task.CompletedTask;
        }

        public Task InvalidateAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            InvalidateAllCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeStudentEventPublisher : IStudentEventPublisher
    {
        public List<StudentCreatedIntegrationEvent> CreatedEvents { get; } = [];
        public List<StudentUpdatedIntegrationEvent> UpdatedEvents { get; } = [];
        public List<StudentDeletedIntegrationEvent> DeletedEvents { get; } = [];

        public Task PublishCreatedAsync(StudentCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            CreatedEvents.Add(integrationEvent);
            return Task.CompletedTask;
        }

        public Task PublishUpdatedAsync(StudentUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            UpdatedEvents.Add(integrationEvent);
            return Task.CompletedTask;
        }

        public Task PublishDeletedAsync(StudentDeletedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            DeletedEvents.Add(integrationEvent);
            return Task.CompletedTask;
        }
    }
}
