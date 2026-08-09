using LMS_Assignment.Api.Controllers.Dtos;
using LMS_Assignment.Application.ClassSubjects;
using LMS_Assignment.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Assignment.Api.Controllers;

[ApiController]
[Route("api/class-subjects")]
public class ClassSubjectsController : ControllerBase
{
    private readonly IClassSubjectService _classSubjectService;

    public ClassSubjectsController(IClassSubjectService classSubjectService)
    {
        _classSubjectService = classSubjectService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClassSubjectResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var classSubjects = await _classSubjectService.GetAllAsync(cancellationToken);

        var response = classSubjects
            .Select(cs => new ClassSubjectResponse(cs.Id, cs.ClassId, cs.Class.Name, cs.SubjectId, cs.Subject.Name))
            .ToList();

        return Ok(response);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    public async Task<ActionResult<ClassSubjectResponse>> Create(CreateClassSubjectRequest request, CancellationToken cancellationToken)
    {
        var classSubject = await _classSubjectService.CreateAsync(request.ClassId, request.SubjectId, cancellationToken);

        var response = new ClassSubjectResponse(
            classSubject.Id,
            classSubject.ClassId,
            classSubject.Class.Name,
            classSubject.SubjectId,
            classSubject.Subject.Name);

        return Created($"/api/class-subjects/{classSubject.Id}", response);
    }
}
