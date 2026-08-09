using LMS_Assignment.Api.Controllers.Dtos;
using LMS_Assignment.Application.Classes;
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

        var response = new ClassResponse(@class.Id, @class.Name, @class.AcademicYear, @class.IsActive);

        return Created($"/api/classes/{@class.Id}", response);
    }
}
