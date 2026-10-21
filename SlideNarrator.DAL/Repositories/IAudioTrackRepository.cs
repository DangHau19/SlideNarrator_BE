using SlideNarrator.DAL.Entities;

namespace SlideNarrator.DAL.Repositories;

public interface IAudioTrackRepository : IGenericRepository<AudioTrack>
{
    /// <summary>Lấy toàn bộ file audio của một dự án (tùy chọn lọc theo ngôn ngữ đích), sắp theo số trang.</summary>
    Task<List<AudioTrack>> GetByProjectAsync(Guid projectId, string? targetLanguage = null);
}
