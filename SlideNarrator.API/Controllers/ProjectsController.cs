using Microsoft.AspNetCore.Mvc;
using SlideNarrator.BLL;
using SlideNarrator.BLL.DTOs;
using SlideNarrator.DAL.Repositories;

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
    private readonly IUnitOfWork _uow;

    public ProjectsController(IProjectService projects, IUnitOfWork uow)
    {
        _projects = projects;
        _uow = uow;
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

    /// <summary>
    /// Gói xem lại dự án: các trang kèm văn bản gốc, bản dịch và audio đã lưu trong MySQL.
    /// Không truyền language thì lấy ngôn ngữ dịch mới nhất của dự án.
    /// </summary>
    [HttpGet("{id:guid}/view")]
    [ProducesResponseType(typeof(ViewPackageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetView(Guid id, [FromQuery] string? language = null)
    {
        var lang = string.IsNullOrWhiteSpace(language) ? null : language.Trim().ToLowerInvariant();
        var project = await _uow.Projects.GetWithAudioAsync(id, lang);
        if (project is null)
            return NotFound(MakeError(404, "PROJECT_NOT_FOUND", "Không tìm thấy dự án."));

        var slides = project.Slides.OrderBy(s => s.PageNumber).ToList();

        var chosen = lang
            ?? slides.SelectMany(s => s.Translations)
                     .OrderByDescending(t => t.UpdatedAt)
                     .Select(t => t.TargetLanguage)
                     .FirstOrDefault()
            ?? string.Empty;

        var pages = slides.Select(s =>
        {
            var translation = s.Translations.FirstOrDefault(t => t.TargetLanguage == chosen);
            var audio = translation?.AudioTracks.OrderByDescending(a => a.CreatedAt).FirstOrDefault();
            return new ViewPageItem(
                s.PageNumber,
                s.ImagePath,
                audio?.FilePath,
                translation?.TranslatedText ?? string.Empty,
                s.NarrationText);
        }).ToList();

        return Ok(new ViewPackageResponse(project.Id, chosen, pages, project.Name, project.SourceLanguage));
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
