using SlideNarrator.DAL.Entities;
using SlideNarrator.DAL.Enums;

namespace SlideNarrator.DAL.Repositories;

public interface IJobRepository : IGenericRepository<ProcessingJob>
{
    /// <summary>Lấy các tác vụ theo trạng thái, cũ nhất trước. Dùng cho tác vụ nền quét job Pending.</summary>
    Task<List<ProcessingJob>> GetByStatusAsync(JobStatus status, int take = 20);

    /// <summary>Lấy các tác vụ của một dự án, mới nhất trước.</summary>
    Task<List<ProcessingJob>> GetByProjectAsync(Guid projectId);
}
