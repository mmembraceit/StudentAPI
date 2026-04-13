using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentApi.Application.Interfaces;
using StudentApi.Application.Students;
using StudentApi.Presentation.Common;

namespace StudentApi.Presentation.Controllers;

/// <summary>
/// REST controller for student endpoints.
/// Delegates business logic to <see cref="IStudentService"/>.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    /// Returns a specific student by id and tenant.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetById(Guid id, [FromQuery] Guid tenantId, CancellationToken cancellationToken)
    {
        var student = await _studentService.GetByIdAsync(id, tenantId, cancellationToken);

        return Ok(ApiResponse<StudentDto>.SuccessResponse(student));
    }

    /// Returns all students associated with a tenant.
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentDto>>>> GetAll([FromQuery] Guid tenantId, CancellationToken cancellationToken)
    {
        var students = await _studentService.GetAllAsync(tenantId, cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<StudentDto>>.SuccessResponse(students));
    }

    /// Creates a new student from the request payload.
    [HttpPost]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Create([FromBody] CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var createdStudent = await _studentService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdStudent.Id, tenantId = createdStudent.TenantId },
            ApiResponse<StudentDto>.SuccessResponse(createdStudent));
    }

    /// Updates an existing student using route id and tenant query scope.
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Update(Guid id, [FromQuery] Guid tenantId, [FromBody] UpdateStudentRequest request, CancellationToken cancellationToken)
    {
        var updatedStudent = await _studentService.UpdateAsync(id, tenantId, request, cancellationToken);

        return Ok(ApiResponse<StudentDto>.SuccessResponse(updatedStudent));
    }

    /// Deletes a student and returns a standard response without data.
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, [FromQuery] Guid tenantId, CancellationToken cancellationToken)
    {
        var request = new DeleteStudentRequest(id, tenantId);
        await _studentService.DeleteAsync(request, cancellationToken);

        return Ok(ApiResponse<object?>.SuccessResponse(null));
    }
}
