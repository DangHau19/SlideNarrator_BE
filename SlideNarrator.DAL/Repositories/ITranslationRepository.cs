using SlideNarrator.DAL.Entities;

namespace SlideNarrator.DAL.Repositories;

public interface ITranslationRepository : IGenericRepository<TranslationRecord>
{
    /// <summary>
    /// Thêm mới hoặc cập nhật bản dịch của một trang theo ngôn ngữ đích
    /// (mỗi trang chỉ có một bản dịch cho mỗi ngôn ngữ). Chưa gọi SaveChanges.
    /// </summary>
    Task<TranslationRecord> UpsertAsync(Guid slidePageId, string targetLanguage, string translatedText);

    /// <summary>Lấy các bản dịch của cả dự án theo ngôn ngữ đích, kèm file audio, sắp theo số trang.</summary>
    Task<List<TranslationRecord>> GetByProjectAndLanguageAsync(Guid projectId, string targetLanguage);
}
