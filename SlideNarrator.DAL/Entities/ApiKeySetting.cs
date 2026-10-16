namespace SlideNarrator.DAL.Entities;

/// <summary>Khóa API của dịch vụ ngoài (OpenAI, AzureSpeech, GoogleTts). Chỉ lưu bản đã mã hóa.</summary>
public class ApiKeySetting
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string EncryptedKey { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
