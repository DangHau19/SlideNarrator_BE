namespace SlideNarrator.DAL.Entities;

/// <summary>Nhật ký hệ thống (khớp LogEntryResponse của BLL).</summary>
public class LogEntry
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Level { get; set; } = "Information";
    public string Action { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }

    public Guid? UserId { get; set; }
    public UserAccount? User { get; set; }
}
