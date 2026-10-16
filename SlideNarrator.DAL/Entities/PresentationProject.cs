namespace SlideNarrator.DAL.Entities;

/// <summary>Dự án slide bài giảng (khớp ProjectResponse của BLL).</summary>
public class PresentationProject
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SourceLanguage { get; set; } = "en";

    /// <summary>Loại file slide gốc: pdf, png, jpg.</summary>
    public string SourceFileType { get; set; } = "pdf";

    /// <summary>Số trang slide hiện có (0 khi chưa tải slide).</summary>
    public int PageCount { get; set; }

    public string? OriginalFilePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UserAccountId { get; set; }
    public UserAccount Owner { get; set; } = null!;

    public ICollection<SlidePage> Slides { get; set; } = new List<SlidePage>();
    public ICollection<ProcessingJob> Jobs { get; set; } = new List<ProcessingJob>();
}
