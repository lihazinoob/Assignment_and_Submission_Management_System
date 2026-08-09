using LMS_Assignment.Api.Controllers.Dtos;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Application.TeacherSubjectAssignments;
using LMS_Assignment.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Assignment.Api.Controllers;

[ApiController]
[Route("api/teacher-subject-assignments")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class TeacherSubjectAssignmentsController : ControllerBase
{
    private readonly ITeacherSubjectAssignmentService _assignmentService;
    private readonly ICurrentUserService _currentUserService;

    public TeacherSubjectAssignmentsController(
        ITeacherSubjectAssignmentService assignmentService,
        ICurrentUserService currentUserService)
    {
        _assignmentService = assignmentService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TeacherSubjectAssignmentResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var assignments = await _assignmentService.GetAllAsync(cancellationToken);

        var response = assignments
            .Select(a => new TeacherSubjectAssignmentResponse(
                a.Id,
                a.TeacherId,
                a.Teacher.FullName,
                a.ClassSubjectId,
                a.ClassSubject.Class.Name,
                a.ClassSubject.Subject.Name,
                a.AssignedBy,
                a.AssignedAt))
            .ToList();

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<TeacherSubjectAssignmentResponse>> Create(
        CreateTeacherSubjectAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var assignment = await _assignmentService.AssignTeacherAsync(
            request.TeacherId,
            request.ClassSubjectId,
            _currentUserService.UserId,
            cancellationToken);

        var response = new TeacherSubjectAssignmentResponse(
            assignment.Id,
            assignment.TeacherId,
            assignment.Teacher.FullName,
            assignment.ClassSubjectId,
            assignment.ClassSubject.Class.Name,
            assignment.ClassSubject.Subject.Name,
            assignment.AssignedBy,
            assignment.AssignedAt);

        return Created($"/api/teacher-subject-assignments/{assignment.Id}", response);
    }
}
