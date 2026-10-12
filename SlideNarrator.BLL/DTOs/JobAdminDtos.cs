using System.ComponentModel.DataAnnotations;

namespace SlideNarrator.BLL.DTOs;

// ===== Nhóm Job và Quản trị (US-21 đến US-23) =====

public enum JobType { SplitSlides, Translate, GenerateAudio, Export }

// Khớp bảng PROCESSING_JOB trong thiết kế: Pending, Processing, Succeeded, Failed
public enum JobStatus { Pending, Processing, Succeeded, Failed }

public record JobAcceptedResponse(
    Guid JobId,
    JobStatus Status);                   // luôn là Pending khi vừa nhận

public record JobResponse(
    Guid Id,
    Guid ProjectId,
    JobType Type,
    JobStatus Status,
    int Progress,                        // 0 đến 100
    string? ErrorMessage,                // chỉ có giá trị khi Failed
    DateTime CreatedAt,
    DateTime? FinishedAt);

public record LogEntryResponse(
    long Id,
    DateTime Timestamp,
    string Level,                        // "Information" | "Warning" | "Error"
    Guid? UserId,
    string Action,
    string Message,
    string? ErrorCode);

public record PagedResponse<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalItems);

public record SaveApiKeyRequest(
    [Required] string Provider,          // "OpenAI" | "AzureSpeech" | "GoogleTts"
    [Required] string ApiKey);

public record ApiKeyResponse(
    string Provider,
    string MaskedKey,                    // ví dụ "sk-****a9f2", không bao giờ trả khóa thật
    DateTime UpdatedAt);

public record ErrorResponse(
    int Status,
    string Error,                        // mã lỗi chữ hoa, ví dụ "VALIDATION_ERROR"
    string Message,
    List<string>? Details,
    DateTime Timestamp);
