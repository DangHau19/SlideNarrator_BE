namespace SlideNarrator.DAL.Entities;

/// <summary>Bản dịch của một trang theo một ngôn ngữ đích (khớp TranslationResponse của BLL).</summary>
public class TranslationRecord
{
    public Guid Id { get; set; }
    public string TargetLanguage { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid SlidePageId { get; set; }
    public SlidePage SlidePage { get; set; } = null!;

    public ICollection<AudioTrack> AudioTracks { get; set; } = new List<AudioTrack>();
}
