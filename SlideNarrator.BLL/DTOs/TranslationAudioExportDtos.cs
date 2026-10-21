using System.ComponentModel.DataAnnotations;

namespace SlideNarrator.BLL.DTOs;

// ===== Nhóm Dịch thuật, TTS và Xuất bản (US-10 đến US-20) =====

public enum VoiceGender { Male, Female }

public enum ExportFormat { Zip, Video }

public record LanguageResponse(
    string Code,                         // "vi", "en", "ja"
    string Name);                        // "Tiếng Việt"

public record TranslateRequest(
    [Required, MinLength(1)] List<string> TargetLanguages);  // US-13: nhiều ngôn ngữ một lần

public record TranslationResponse(
    int PageNumber,
    string Language,
    string OriginalText,
    string TranslatedText,
    DateTime UpdatedAt);

public record UpdateTranslationRequest(
    [Required] string TranslatedText);   // bản sửa tay thay thế bản dịch tự động

public record GenerateAudioRequest(
    [Required] string TargetLanguage,
    VoiceGender VoiceGender = VoiceGender.Female,
    [Range(0.5, 2.0)] double Speed = 1.0);

public record ExportRequest(
    [Required] string TargetLanguage,
    ExportFormat Format);

public record ViewPageItem(
    int PageNumber,
    string ImageUrl,
    string? AudioUrl,
    string TranslatedText,
    string? OriginalText = null);        // văn bản thuyết minh gốc

public record ViewPackageResponse(
    Guid ProjectId,
    string Language,
    List<ViewPageItem> Pages,
    string? ProjectName = null,
    string? SourceLanguage = null);
