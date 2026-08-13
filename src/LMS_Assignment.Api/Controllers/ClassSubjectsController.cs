using LMS_Assignment.Api.Controllers.Dtos;
using LMS_Assignment.Application.ClassSubjects;
using LMS_Assignment.Application.Common.Models;
using LMS_Assignment.Domain.Entities;
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
    public async Task<ActionResult<PagedResponse<ClassSubjectResponse>>> GetAll(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await _classSubjectService.GetAllAsync(new PaginationQuery { Page = page, PageSize = pageSize }, cancellationToken);

        var items = result.Items.Select(ToResponse).ToList();

        return Ok(new PagedResponse<ClassSubjectResponse>(items, result.TotalCount, result.Page, result.PageSize));
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    public async Task<ActionResult<ClassSubjectResponse>> Create(CreateClassSubjectRequest request, CancellationToken cancellationToken)
    {
        var classSubject = await _classSubjectService.CreateAsync(request.ClassId, request.SubjectId, cancellationToken);

        return Created($"/api/class-subjects/{classSubject.Id}", ToResponse(classSubject));
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<ClassSubjectResponse>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var classSubject = await _classSubjectService.DeactivateAsync(id, cancellationToken);

        return Ok(ToResponse(classSubject));
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<ClassSubjectResponse>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var classSubject = await _classSubjectService.ActivateAsync(id, cancellationToken);

        return Ok(ToResponse(classSubject));
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _classSubjectService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }

    private static ClassSubjectResponse ToResponse(ClassSubject classSubject) =>
        new(classSubject.Id, classSubject.ClassId, classSubject.Class.Name, classSubject.SubjectId, classSubject.Subject.Name, classSubject.IsActive);
}
