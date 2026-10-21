using SlideNarrator.DAL.Entities;

namespace SlideNarrator.DAL.Repositories;

/// <summary>Nhật ký hệ thống. Khóa của LOG_ENTRY là số nguyên (long) nên không dùng GenericRepository.</summary>
public interface ILogRepository
{
    /// <summary>Thêm một dòng nhật ký (chưa SaveChanges; lưu cùng giao dịch với nghiệp vụ).</summary>
    Task WriteAsync(string level, string action, string message, Guid? userId = null, string? errorCode = null);

    /// <summary>Lấy nhật ký phân trang, mới nhất trước; có thể lọc theo mức độ.</summary>
    Task<(List<LogEntry> Items, int Total)> GetPagedAsync(int page, int pageSize, string? level = null);
}
