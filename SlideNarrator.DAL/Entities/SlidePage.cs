namespace SlideNarrator.DAL.Entities;

/// <summary>Một trang slide (khớp SlidePageResponse của BLL).</summary>
public class SlidePage
{
    public Guid Id { get; set; }
    public int PageNumber { get; set; }
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>Văn bản thuyết minh gốc; null nếu chưa nhập.</summary>
    public string? NarrationText { get; set; }

    public Guid PresentationProjectId { get; set; }
    public PresentationProject Project { get; set; } = null!;

    public ICollection<TranslationRecord> Translations { get; set; } = new List<TranslationRecord>();
}
