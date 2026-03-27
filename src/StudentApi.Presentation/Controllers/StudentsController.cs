using Microsoft.AspNetCore.Mvc;
using StudentApi.Application.Interfaces;
using StudentApi.Application.Students;

namespace StudentApi.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentDto>> GetById(Guid id, [FromQuery] Guid tenantId, CancellationToken cancellationToken)
    {
        var student = await _studentService.GetByIdAsync(id, tenantId, cancellationToken);

        if (student is null)
        {
            return NotFound(); // Returns a 404 if the student is not found
        }

        return Ok(student); // Returns a 200 with the student data in the response body
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StudentDto>>> GetAll([FromQuery] Guid tenantId, CancellationToken cancellationToken)
    {
        var students = await _studentService.GetAllAsync(tenantId, cancellationToken);

        return Ok(students); // Returns a 200 with the list of students in the response body
    }

    [HttpPost]
    public async Task<ActionResult<StudentDto>> Create([FromBody] CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var createdStudent = await _studentService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdStudent.Id, tenantId = createdStudent.TenantId },
            createdStudent); // Returns a 201 with the created student data in the response body and a Location header pointing to the new resource
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StudentDto>> Update(Guid id, [FromQuery] Guid tenantId, [FromBody] UpdateStudentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updatedStudent = await _studentService.UpdateAsync(id, tenantId, request, cancellationToken);
            return Ok(updatedStudent);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(); // Returns a 404  if the student to update is not found
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var request = new DeleteStudentRequest(id, tenantId);
            await _studentService.DeleteAsync(request, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        { 
            return NotFound(); // Returns a 404 if the student to delete is not found
        }
    }
}
