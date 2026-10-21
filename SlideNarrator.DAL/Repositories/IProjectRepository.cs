using SlideNarrator.DAL.Entities;

namespace SlideNarrator.DAL.Repositories;

public interface IProjectRepository : IGenericRepository<PresentationProject>
{
    /// <summary>Lấy dự án kèm danh sách trang slide và tác vụ nền.</summary>
    Task<PresentationProject?> GetWithSlidesAndJobsAsync(Guid id);

    /// <summary>Lấy dự án kèm toàn bộ cây dữ liệu: trang slide, bản dịch, file giọng đọc.</summary>
    Task<PresentationProject?> GetFullAsync(Guid id);

    /// <summary>Lấy các dự án của một người dùng, mới nhất trước.</summary>
    Task<List<PresentationProject>> GetByOwnerAsync(Guid userId);
    Task<PresentationProject?> GetWithAudioAsync(Guid id, string? targetLanguage = null);
}
