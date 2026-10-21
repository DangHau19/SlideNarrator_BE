using SlideNarrator.DAL.Entities;

namespace SlideNarrator.DAL.Repositories;

public interface ISlideRepository : IGenericRepository<SlidePage>
{
    /// <summary>Lấy các trang của một dự án, sắp theo số trang (không theo dõi thay đổi).</summary>
    Task<List<SlidePage>> GetByProjectAsync(Guid projectId);

    /// <summary>Lấy một trang theo (dự án, số trang); null nếu không có.</summary>
    Task<SlidePage?> GetByPageNumberAsync(Guid projectId, int pageNumber);
}
