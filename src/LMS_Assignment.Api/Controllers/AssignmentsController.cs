using LMS_Assignment.Api.Controllers.Dtos;
using LMS_Assignment.Application.Assignments;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Assignment.Api.Controllers;

[ApiController]
[Route("api/assignments")]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;
    private readonly ICurrentUserService _currentUserService;

    public AssignmentsController(IAssignmentService assignmentService, ICurrentUserService currentUserService)
    {
        _assignmentService = assignmentService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AssignmentResponse>>> GetAll(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] AssignmentStatus? status,
        [FromQuery] Guid? classSubjectId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var filter = new AssignmentFilter
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            ClassSubjectId = classSubjectId,
            Search = search
        };

        var result = await _assignmentService.GetForCurrentUserAsync(
            _currentUserService.UserId!.Value,
            _currentUserService.Role!.Value,
            filter,
            cancellationToken);

        return Ok(new PagedResponse<AssignmentResponse>(
            result.Items.Select(ToResponse).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssignmentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentService.GetByIdAsync(
            id,
            _currentUserService.UserId!.Value,
            _currentUserService.Role!.Value,
            cancellationToken);

        return Ok(ToResponse(assignment));
    }

    [Authorize(Roles = nameof(UserRole.Teacher))]
    [HttpPost]
    public async Task<ActionResult<AssignmentResponse>> Create(CreateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentService.CreateAsync(
            request.TeacherSubjectAssignmentId,
            request.Title,
            request.Description,
            request.Deadline,
            request.MaxMarks,
            request.AllowResubmission,
            _currentUserService.UserId!.Value,
            cancellationToken);

        return Created($"/api/assignments/{assignment.Id}", ToResponse(assignment));
    }

    [Authorize(Roles = nameof(UserRole.Teacher))]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AssignmentResponse>> Update(Guid id, UpdateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentService.UpdateAsync(
            id,
            request.Title,
            request.Description,
            request.Deadline,
            request.MaxMarks,
            request.AllowResubmission,
            _currentUserService.UserId!.Value,
            cancellationToken);

        return Ok(ToResponse(assignment));
    }

    [Authorize(Roles = nameof(UserRole.Teacher))]
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<AssignmentResponse>> Publish(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentService.PublishAsync(id, _currentUserService.UserId!.Value, cancellationToken);

        return Ok(ToResponse(assignment));
    }

    [Authorize(Roles = nameof(UserRole.Teacher))]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _assignmentService.DeleteAsync(id, _currentUserService.UserId!.Value, cancellationToken);

        return NoContent();
    }

    private static AssignmentResponse ToResponse(Assignment assignment)
    {
        var teacherSubjectAssignment = assignment.TeacherSubjectAssignment;

        return new AssignmentResponse(
            assignment.Id,
            assignment.TeacherSubjectAssignmentId,
            teacherSubjectAssignment.Teacher.FullName,
            teacherSubjectAssignment.ClassSubject.Class.Name,
            teacherSubjectAssignment.ClassSubject.Subject.Name,
            assignment.Title,
            assignment.Description,
            assignment.Deadline,
            assignment.MaxMarks,
            assignment.AllowResubmission,
            assignment.Status);
    }
}
