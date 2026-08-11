using LMS_Assignment.Api.Controllers.Dtos;
using LMS_Assignment.Application.Classes;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Assignment.Api.Controllers;

[ApiController]
[Route("api/classes")]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassesController(IClassService classService)
    {
        _classService = classService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClassResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var classes = await _classService.GetAllAsync(cancellationToken);

        var response = classes
            .Select(c => new ClassResponse(c.Id, c.Name, c.AcademicYear, c.IsActive))
            .ToList();

        return Ok(response);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    public async Task<ActionResult<ClassResponse>> Create(CreateClassRequest request, CancellationToken cancellationToken)
    {
        var @class = await _classService.CreateAsync(request.Name, request.AcademicYear, cancellationToken);

        return Created($"/api/classes/{@class.Id}", ToResponse(@class));
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClassResponse>> Update(Guid id, UpdateClassRequest request, CancellationToken cancellationToken)
    {
        var @class = await _classService.UpdateAsync(id, request.Name, request.AcademicYear, cancellationToken);

        return Ok(ToResponse(@class));
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<ClassResponse>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var @class = await _classService.DeactivateAsync(id, cancellationToken);

        return Ok(ToResponse(@class));
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<ClassResponse>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var @class = await _classService.ActivateAsync(id, cancellationToken);

        return Ok(ToResponse(@class));
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _classService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }

    private static ClassResponse ToResponse(Class @class) =>
        new(@class.Id, @class.Name, @class.AcademicYear, @class.IsActive);
}
