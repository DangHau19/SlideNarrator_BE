using Microsoft.EntityFrameworkCore;
using SlideNarrator.DAL.Data;
using SlideNarrator.DAL.Entities;

namespace SlideNarrator.DAL.Repositories;

public class ProjectRepository : GenericRepository<PresentationProject>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context) { }

    public Task<PresentationProject?> GetWithSlidesAndJobsAsync(Guid id) =>
        Set.Include(p => p.Slides)
           .Include(p => p.Jobs)
           .AsSplitQuery()
           .FirstOrDefaultAsync(p => p.Id == id);

    public Task<PresentationProject?> GetFullAsync(Guid id) =>
        Set.Include(p => p.Slides)
               .ThenInclude(s => s.Translations)
                   .ThenInclude(t => t.AudioTracks)
           .AsSplitQuery()
           .FirstOrDefaultAsync(p => p.Id == id);

    public Task<PresentationProject?> GetWithAudioAsync(Guid id, string? targetLanguage = null)
    {
        IQueryable<PresentationProject> query = Set;

        if (string.IsNullOrEmpty(targetLanguage))
        {
            query = query.Include(p => p.Slides.OrderBy(s => s.PageNumber))
                             .ThenInclude(s => s.Translations)
                                 .ThenInclude(t => t.AudioTracks);
        }
        else
        {
            query = query.Include(p => p.Slides.OrderBy(s => s.PageNumber))
                             .ThenInclude(s => s.Translations.Where(t => t.TargetLanguage == targetLanguage))
                                 .ThenInclude(t => t.AudioTracks);
        }

        return query.AsNoTracking().AsSplitQuery().FirstOrDefaultAsync(p => p.Id == id);
    }

    public Task<List<PresentationProject>> GetByOwnerAsync(Guid userId) =>
        Set.AsNoTracking()
           .Where(p => p.UserAccountId == userId)
           .OrderByDescending(p => p.CreatedAt)
           .ToListAsync();
}
