namespace SlideNarrator.DAL.Entities;

/// <summary>File giọng đọc TTS của một bản dịch (khớp GenerateAudioRequest của BLL).</summary>
public class AudioTrack
{
    public Guid Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string VoiceName { get; set; } = string.Empty;

    /// <summary>Giới tính giọng: Male hoặc Female.</summary>
    public string VoiceGender { get; set; } = "Female";

    public double Speed { get; set; } = 1.0;
    public double? DurationSeconds { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid TranslationRecordId { get; set; }
    public TranslationRecord TranslationRecord { get; set; } = null!;
}
