using SlideNarrator.DAL.Entities;
using SlideNarrator.DAL.Enums;

namespace SlideNarrator.DAL.Repositories;

public interface IJobRepository : IGenericRepository<ProcessingJob>
{
    /// <summary>Lấy các tác vụ theo trạng thái, cũ nhất trước. Dùng cho tác vụ nền quét job Pending.</summary>
    Task<List<ProcessingJob>> GetByStatusAsync(JobStatus status, int take = 20);

    /// <summary>Lấy các tác vụ của một dự án, mới nhất trước.</summary>
    Task<List<ProcessingJob>> GetByProjectAsync(Guid projectId);

    // Các hàm chuyển trạng thái: chỉ sửa entity đang theo dõi, người gọi tự SaveChangesAsync.

    /// <summary>Pending -> Processing, ghi StartedAt, Progress = 0.</summary>
    void Start(ProcessingJob job, string? step = null);

    /// <summary>Cập nhật tiến độ (tự giới hạn trong 0..100) và bước đang xử lý.</summary>
    void ReportProgress(ProcessingJob job, int percent, string? step = null);

    /// <summary>Processing -> Succeeded, Progress = 100, ghi FinishedAt và đường dẫn kết quả.</summary>
    void Complete(ProcessingJob job, string? outputFilePath = null);

    /// <summary>Processing -> Failed, ghi FinishedAt và thông báo lỗi.</summary>
    void Fail(ProcessingJob job, string errorMessage);
}
