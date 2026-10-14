using SlideNarrator.BLL.DTOs;

namespace SlideNarrator.BLL;

/// <summary>
/// Nghiệp vụ quản lý dự án bài giảng (US-05, US-09).
/// Hiện có bản lưu trong bộ nhớ (InMemoryProjectService) để chạy thử API;
/// khi DAL của TV2 xong, viết thêm bản dùng Repository/Unit of Work rồi đổi dòng đăng ký trong Program.cs.
/// </summary>
public interface IProjectService
{
    ProjectResponse Create(CreateProjectRequest request);

    IReadOnlyList<ProjectResponse> GetAll();

    ProjectResponse? GetById(Guid id);

    /// <returns>true nếu xóa được, false nếu không tìm thấy dự án.</returns>
    bool Delete(Guid id);
}
