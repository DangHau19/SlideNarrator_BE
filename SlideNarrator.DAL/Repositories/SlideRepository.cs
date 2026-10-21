using Microsoft.EntityFrameworkCore;
using SlideNarrator.DAL.Data;
using SlideNarrator.DAL.Entities;

namespace SlideNarrator.DAL.Repositories;

public class SlideRepository : GenericRepository<SlidePage>, ISlideRepository
{
    public SlideRepository(AppDbContext context) : base(context) { }

    public Task<List<SlidePage>> GetByProjectAsync(Guid projectId) =>
        Set.AsNoTracking()
           .Where(s => s.PresentationProjectId == projectId)
           .OrderBy(s => s.PageNumber)
           .ToListAsync();

    public Task<SlidePage?> GetByPageNumberAsync(Guid projectId, int pageNumber) =>
        Set.FirstOrDefaultAsync(s => s.PresentationProjectId == projectId && s.PageNumber == pageNumber);
}
