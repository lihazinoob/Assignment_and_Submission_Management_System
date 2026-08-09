using LMS_Assignment.Api.Controllers.Dtos;
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
    public async Task<ActionResult<List<SubjectResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var subjects = await _subjectService.GetAllAsync(cancellationToken);

        var response = subjects
            .Select(s => new SubjectResponse(s.Id, s.Name, s.Code, s.IsActive))
            .ToList();

        return Ok(response);
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
