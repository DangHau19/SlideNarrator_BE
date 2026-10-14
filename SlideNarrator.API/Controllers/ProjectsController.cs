using Microsoft.AspNetCore.Mvc;
using SlideNarrator.BLL;
using SlideNarrator.BLL.DTOs;

namespace SlideNarrator.API.Controllers;

/// <summary>
/// Quản lý dự án bài giảng (US-05, US-09) theo tài liệu Đặc tả API.
/// Chưa gắn [Authorize] vì Auth chưa làm; khi có JWT thì thêm [Authorize(Roles = "Presenter")].
/// </summary>
[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projects;

    public ProjectsController(IProjectService projects)
    {
        _projects = projects;
    }

    /// <summary>Tạo dự án bài giảng mới (US-05).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateProjectRequest request)
    {
        try
        {
            var project = _projects.Create(request);
            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(MakeError(400, "VALIDATION_ERROR", ex.Message));
        }
    }

    /// <summary>Xem danh sách dự án (US-09).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll() => Ok(_projects.GetAll());

    /// <summary>Xem một dự án; dùng cho header Location sau khi tạo.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var project = _projects.GetById(id);
        return project is null
            ? NotFound(MakeError(404, "PROJECT_NOT_FOUND", "Không tìm thấy dự án."))
            : Ok(project);
    }

    /// <summary>Xóa dự án (US-09).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        return _projects.Delete(id)
            ? NoContent()
            : NotFound(MakeError(404, "PROJECT_NOT_FOUND", "Không tìm thấy dự án."));
    }

    private static ErrorResponse MakeError(int status, string error, string message) =>
        new(status, error, message, null, DateTime.UtcNow);
}
