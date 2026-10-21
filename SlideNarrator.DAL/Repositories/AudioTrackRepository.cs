using Microsoft.EntityFrameworkCore;
using SlideNarrator.DAL.Data;
using SlideNarrator.DAL.Entities;

namespace SlideNarrator.DAL.Repositories;

public class AudioTrackRepository : GenericRepository<AudioTrack>, IAudioTrackRepository
{
    public AudioTrackRepository(AppDbContext context) : base(context) { }

    public Task<List<AudioTrack>> GetByProjectAsync(Guid projectId, string? targetLanguage = null)
    {
        var query = Set.AsNoTracking()
            .Include(a => a.TranslationRecord).ThenInclude(t => t.SlidePage)
            .Where(a => a.TranslationRecord.SlidePage.PresentationProjectId == projectId);

        if (!string.IsNullOrEmpty(targetLanguage))
            query = query.Where(a => a.TranslationRecord.TargetLanguage == targetLanguage);

        return query.OrderBy(a => a.TranslationRecord.SlidePage.PageNumber).ToListAsync();
    }
}
