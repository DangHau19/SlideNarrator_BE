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

    public void Start(ProcessingJob job, string? step = null)
    {
        job.Status = JobStatus.Processing;
        job.Progress = 0;
        job.CurrentStep = step;
        job.StartedAt = DateTime.UtcNow;
    }

    public void ReportProgress(ProcessingJob job, int percent, string? step = null)
    {
        job.Progress = Math.Clamp(percent, 0, 100);
        if (step is not null) job.CurrentStep = step;
    }

    public void Complete(ProcessingJob job, string? outputFilePath = null)
    {
        job.Status = JobStatus.Succeeded;
        job.Progress = 100;
        job.OutputFilePath = outputFilePath;
        job.FinishedAt = DateTime.UtcNow;
    }

    public void Fail(ProcessingJob job, string errorMessage)
    {
        job.Status = JobStatus.Failed;
        job.ErrorMessage = errorMessage.Length > 2000 ? errorMessage[..2000] : errorMessage;
        job.FinishedAt = DateTime.UtcNow;
    }
}
