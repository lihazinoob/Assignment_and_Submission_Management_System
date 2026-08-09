using LMS_Assignment.Api.Controllers.Dtos;
using LMS_Assignment.Application.StudentEnrollments;
using LMS_Assignment.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Assignment.Api.Controllers;

[ApiController]
[Route("api/student-enrollments")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class StudentEnrollmentsController : ControllerBase
{
    private readonly IStudentEnrollmentService _enrollmentService;

    public StudentEnrollmentsController(IStudentEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<ActionResult<List<StudentEnrollmentResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var enrollments = await _enrollmentService.GetAllAsync(cancellationToken);

        var response = enrollments
            .Select(e => new StudentEnrollmentResponse(
                e.Id,
                e.StudentId,
                e.Student.FullName,
                e.ClassId,
                e.Class.Name,
                e.RollNumber,
                e.Status,
                e.EnrolledAt))
            .ToList();

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<StudentEnrollmentResponse>> Create(CreateStudentEnrollmentRequest request, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentService.EnrollAsync(
            request.StudentId,
            request.ClassId,
            request.RollNumber,
            cancellationToken);

        var response = new StudentEnrollmentResponse(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.Student.FullName,
            enrollment.ClassId,
            enrollment.Class.Name,
            enrollment.RollNumber,
            enrollment.Status,
            enrollment.EnrolledAt);

        return Created($"/api/student-enrollments/{enrollment.Id}", response);
    }
}
