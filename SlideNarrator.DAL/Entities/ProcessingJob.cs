using SlideNarrator.DAL.Enums;

namespace SlideNarrator.DAL.Entities;

/// <summary>Tác vụ nền (khớp JobResponse của BLL).</summary>
public class ProcessingJob
{
    public Guid Id { get; set; }
    public JobType Type { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Pending;

    /// <summary>Tiến độ 0 đến 100.</summary>
    public int Progress { get; set; }

    public string? TargetLanguage { get; set; }
    public string? CurrentStep { get; set; }
    public string? OutputFilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public Guid PresentationProjectId { get; set; }
    public PresentationProject Project { get; set; } = null!;
}
