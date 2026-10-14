using System.Collections.Concurrent;
using SlideNarrator.BLL.DTOs;

namespace SlideNarrator.BLL;

/// <summary>
/// Bản tạm: lưu dự án trong bộ nhớ, mất khi tắt ứng dụng.
/// Chưa có xác thực nên chưa lọc theo chủ sở hữu; US-09 "chỉ thấy dự án của mình" sẽ làm khi có Auth.
/// </summary>
public class InMemoryProjectService : IProjectService
{
    // Danh sách ngôn ngữ hỗ trợ tạm thời, khớp ví dụ trong đặc tả API.
    public static readonly IReadOnlySet<string> SupportedLanguages =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "vi", "en", "ja", "ko", "zh", "fr", "de" };

    private readonly ConcurrentDictionary<Guid, ProjectResponse> _projects = new();

    public ProjectResponse Create(CreateProjectRequest request)
    {
        var language = request.SourceLanguage.Trim().ToLowerInvariant();
        if (!SupportedLanguages.Contains(language))
        {
            throw new ArgumentException(
                $"Ngôn ngữ '{request.SourceLanguage}' chưa được hỗ trợ. Hỗ trợ: {string.Join(", ", SupportedLanguages)}.");
        }

        var project = new ProjectResponse(
            Id: Guid.NewGuid(),
            Name: request.Name.Trim(),
            SourceLanguage: language,
            PageCount: 0,
            CreatedAt: DateTime.UtcNow);

        _projects[project.Id] = project;
        return project;
    }

    public IReadOnlyList<ProjectResponse> GetAll() =>
        _projects.Values.OrderByDescending(p => p.CreatedAt).ToList();

    public ProjectResponse? GetById(Guid id) =>
        _projects.TryGetValue(id, out var project) ? project : null;

    public bool Delete(Guid id) => _projects.TryRemove(id, out _);
}
