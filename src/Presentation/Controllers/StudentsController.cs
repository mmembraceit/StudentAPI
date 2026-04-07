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

    /// <summary>
    /// Returns a specific student by id and tenant.
    /// </summary>
    /// <param name="id">Student identifier.</param>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>A wrapped student response.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetById(Guid id, [FromQuery] Guid tenantId, CancellationToken cancellationToken)
    {
        var student = await _studentService.GetByIdAsync(id, tenantId, cancellationToken);

        return Ok(ApiResponse<StudentDto>.SuccessResponse(student));
    }

    /// <summary>
    /// Returns all students associated with a tenant.
    /// </summary>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>A wrapped list of students.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentDto>>>> GetAll([FromQuery] Guid tenantId, CancellationToken cancellationToken)
    {
        var students = await _studentService.GetAllAsync(tenantId, cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<StudentDto>>.SuccessResponse(students));
    }

    /// <summary>
    /// Creates a new student from the request payload.
    /// </summary>
    /// <param name="request">Student creation payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>A wrapped created student response.</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Create([FromBody] CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var createdStudent = await _studentService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdStudent.Id, tenantId = createdStudent.TenantId },
            ApiResponse<StudentDto>.SuccessResponse(createdStudent));
    }

    /// <summary>
    /// Updates an existing student using route id and tenant query scope.
    /// </summary>
    /// <param name="id">Student identifier.</param>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="request">Student update payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>A wrapped updated student response.</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Update(Guid id, [FromQuery] Guid tenantId, [FromBody] UpdateStudentRequest request, CancellationToken cancellationToken)
    {
        var updatedStudent = await _studentService.UpdateAsync(id, tenantId, request, cancellationToken);

        return Ok(ApiResponse<StudentDto>.SuccessResponse(updatedStudent));
    }

    /// <summary>
    /// Deletes a student and returns a standard response without data.
    /// </summary>
    /// <param name="id">Student identifier.</param>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>A wrapped empty success response.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, [FromQuery] Guid tenantId, CancellationToken cancellationToken)
    {
        var request = new DeleteStudentRequest(id, tenantId);
        await _studentService.DeleteAsync(request, cancellationToken);

        return Ok(ApiResponse<object?>.SuccessResponse(null));
    }
}
