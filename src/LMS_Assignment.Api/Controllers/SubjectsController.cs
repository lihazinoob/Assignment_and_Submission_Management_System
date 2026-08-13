using LMS_Assignment.Api.Controllers.Dtos;
using LMS_Assignment.Application.Common.Models;
using LMS_Assignment.Application.Subjects;
using LMS_Assignment.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Assignment.Api.Controllers;

[ApiController]
[Route("api/subjects")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<SubjectResponse>>> GetAll(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await _subjectService.GetAllAsync(new PaginationQuery { Page = page, PageSize = pageSize }, cancellationToken);

        var items = result.Items
            .Select(s => new SubjectResponse(s.Id, s.Name, s.Code, s.IsActive))
            .ToList();

        return Ok(new PagedResponse<SubjectResponse>(items, result.TotalCount, result.Page, result.PageSize));
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    public async Task<ActionResult<SubjectResponse>> Create(CreateSubjectRequest request, CancellationToken cancellationToken)
    {
        var subject = await _subjectService.CreateAsync(request.Name, request.Code, cancellationToken);

        var response = new SubjectResponse(subject.Id, subject.Name, subject.Code, subject.IsActive);

        return Created($"/api/subjects/{subject.Id}", response);
    }
}
