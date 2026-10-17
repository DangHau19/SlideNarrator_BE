using SlideNarrator.BLL.DTOs;
using SlideNarrator.DAL.Entities;
using SlideNarrator.DAL.Repositories;

namespace SlideNarrator.BLL;

/// <summary>
/// Bản dùng cơ sở dữ liệu thật (qua Unit of Work của DAL), thay cho InMemoryProjectService.
/// Giữ nguyên interface IProjectService của nhóm (đồng bộ) nên không phải sửa Controller.
/// Chưa có xác thực: mọi dự án gắn với tài khoản "demo"; khi có JWT thì lấy chủ sở hữu từ người dùng hiện tại.
/// </summary>
public class ProjectService : IProjectService
{
    private const string DemoEmail = "demo@slidenarrator.local";

    private readonly IUnitOfWork _uow;

    public ProjectService(IUnitOfWork uow) => _uow = uow;

    public ProjectResponse Create(CreateProjectRequest request)
    {
        var language = request.SourceLanguage.Trim().ToLowerInvariant();
        if (!InMemoryProjectService.SupportedLanguages.Contains(language))
        {
            throw new ArgumentException(
                $"Ngôn ngữ '{request.SourceLanguage}' chưa được hỗ trợ. Hỗ trợ: {string.Join(", ", InMemoryProjectService.SupportedLanguages)}.");
        }

        var owner = GetOrCreateDemoUser();
        var project = new PresentationProject
        {
            Name = request.Name.Trim(),
            SourceLanguage = language,
            UserAccountId = owner.Id
        };

        _uow.Projects.AddAsync(project).GetAwaiter().GetResult();
        _uow.SaveChangesAsync().GetAwaiter().GetResult();
        return ToResponse(project);
    }

    public IReadOnlyList<ProjectResponse> GetAll() =>
        _uow.Projects.GetAllAsync().GetAwaiter().GetResult()
            .OrderByDescending(p => p.CreatedAt)
            .Select(ToResponse)
            .ToList();

    public ProjectResponse? GetById(Guid id)
    {
        var project = _uow.Projects.GetByIdAsync(id).GetAwaiter().GetResult();
        return project is null ? null : ToResponse(project);
    }

    public bool Delete(Guid id)
    {
        var project = _uow.Projects.GetByIdAsync(id).GetAwaiter().GetResult();
        if (project is null) return false;

        _uow.Projects.Remove(project);   // Cascade: xóa luôn slide, bản dịch, audio, job
        _uow.SaveChangesAsync().GetAwaiter().GetResult();
        return true;
    }

    private UserAccount GetOrCreateDemoUser()
    {
        var user = _uow.Users.GetByEmailAsync(DemoEmail).GetAwaiter().GetResult();
        if (user is not null) return user;

        var role = _uow.Roles.FindAsync(r => r.Name == "Presenter").GetAwaiter().GetResult().Single();
        user = new UserAccount
        {
            Email = DemoEmail,
            FullName = "Demo",
            PasswordHash = "chua-co-dang-nhap",
            RoleId = role.Id
        };
        _uow.Users.AddAsync(user).GetAwaiter().GetResult();
        _uow.SaveChangesAsync().GetAwaiter().GetResult();
        return user;
    }

    private static ProjectResponse ToResponse(PresentationProject p) =>
        new(p.Id, p.Name, p.SourceLanguage, p.PageCount, p.CreatedAt);
}
