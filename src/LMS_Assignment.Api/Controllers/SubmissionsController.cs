using LMS_Assignment.Api.Controllers.Dtos;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Application.Submissions;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Assignment.Api.Controllers;

[ApiController]
[Route("api/submissions")]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionService _submissionService;
    private readonly ICurrentUserService _currentUserService;

    public SubmissionsController(ISubmissionService submissionService, ICurrentUserService currentUserService)
    {
        _submissionService = submissionService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<SubmissionResponse>>> GetAll(
        [FromQuery] Guid? assignmentId,
        [FromQuery] SubmissionStatus? status,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var filter = new SubmissionFilter
        {
            AssignmentId = assignmentId,
            Status = status,
            Page = page,
            PageSize = pageSize
        };

        var result = await _submissionService.GetForCurrentUserAsync(
            _currentUserService.UserId!.Value,
            _currentUserService.Role!.Value,
            filter,
            cancellationToken);

        return Ok(new PagedResponse<SubmissionResponse>(
            result.Items.Select(ToResponse).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubmissionResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var submission = await _submissionService.GetByIdAsync(
            id,
            _currentUserService.UserId!.Value,
            _currentUserService.Role!.Value,
            cancellationToken);

        return Ok(ToResponse(submission));
    }

    [Authorize(Roles = nameof(UserRole.Student))]
    [HttpPost]
    public async Task<ActionResult<SubmissionResponse>> Create(CreateSubmissionRequest request, CancellationToken cancellationToken)
    {
        var submission = await _submissionService.SubmitAsync(
            request.AssignmentId,
            request.AnswerText,
            _currentUserService.UserId!.Value,
            cancellationToken);

        return Created($"/api/submissions/{submission.Id}", ToResponse(submission));
    }

    [Authorize(Roles = nameof(UserRole.Student))]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SubmissionResponse>> Update(Guid id, UpdateSubmissionRequest request, CancellationToken cancellationToken)
    {
        var submission = await _submissionService.UpdateAsync(
            id,
            request.AnswerText,
            _currentUserService.UserId!.Value,
            cancellationToken);

        return Ok(ToResponse(submission));
    }

    [Authorize(Roles = nameof(UserRole.Teacher))]
    [HttpPost("{id:guid}/grade")]
    public async Task<ActionResult<SubmissionResponse>> Grade(Guid id, GradeSubmissionRequest request, CancellationToken cancellationToken)
    {
        var submission = await _submissionService.GradeSubmissionAsync(
            id,
            request.MarksObtained,
            request.Feedback,
            _currentUserService.UserId!.Value,
            cancellationToken);

        return Ok(ToResponse(submission));
    }

    private static SubmissionResponse ToResponse(Submission submission)
    {
        return new SubmissionResponse(
            submission.Id,
            submission.AssignmentId,
            submission.Assignment.Title,
            submission.StudentId,
            submission.Student.FullName,
            submission.AnswerText,
            submission.SubmittedAt,
            submission.Status,
            submission.MarksObtained,
            submission.Feedback,
            submission.GradedBy,
            submission.GradedAt);
    }
}
