using Microsoft.EntityFrameworkCore;
using SlideNarrator.DAL.Data;
using SlideNarrator.DAL.Entities;

namespace SlideNarrator.DAL.Repositories;

public class TranslationRepository : GenericRepository<TranslationRecord>, ITranslationRepository
{
    public TranslationRepository(AppDbContext context) : base(context) { }

    public async Task<TranslationRecord> UpsertAsync(Guid slidePageId, string targetLanguage, string translatedText)
    {
        var existing = await Set.FirstOrDefaultAsync(t =>
            t.SlidePageId == slidePageId && t.TargetLanguage == targetLanguage);

        if (existing is not null)
        {
            existing.TranslatedText = translatedText;
            existing.UpdatedAt = DateTime.UtcNow;
            return existing;
        }

        var created = new TranslationRecord
        {
            SlidePageId = slidePageId,
            TargetLanguage = targetLanguage,
            TranslatedText = translatedText
        };
        await Set.AddAsync(created);
        return created;
    }

    public Task<List<TranslationRecord>> GetByProjectAndLanguageAsync(Guid projectId, string targetLanguage) =>
        Set.AsNoTracking()
           .Include(t => t.SlidePage)
           .Include(t => t.AudioTracks)
           .Where(t => t.SlidePage.PresentationProjectId == projectId && t.TargetLanguage == targetLanguage)
           .OrderBy(t => t.SlidePage.PageNumber)
           .ToListAsync();
}
