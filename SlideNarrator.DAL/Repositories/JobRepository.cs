using Microsoft.EntityFrameworkCore;
using SlideNarrator.DAL.Data;
using SlideNarrator.DAL.Entities;
using SlideNarrator.DAL.Enums;

namespace SlideNarrator.DAL.Repositories;

public class JobRepository : GenericRepository<ProcessingJob>, IJobRepository
{
    public JobRepository(AppDbContext context) : base(context) { }

    public Task<List<ProcessingJob>> GetByStatusAsync(JobStatus status, int take = 20) =>
        Set.Where(j => j.Status == status)
           .OrderBy(j => j.CreatedAt)
           .Take(take)
           .ToListAsync();

    public Task<List<ProcessingJob>> GetByProjectAsync(Guid projectId) =>
        Set.AsNoTracking()
           .Where(j => j.PresentationProjectId == projectId)
           .OrderByDescending(j => j.CreatedAt)
           .ToListAsync();
}
