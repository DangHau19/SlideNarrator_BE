using System.ComponentModel.DataAnnotations;

namespace SlideNarrator.BLL.DTOs;

// ===== Nhóm Dự án và Slide (US-05 đến US-09) =====

public record CreateProjectRequest(
    [Required, MaxLength(200)] string Name,
    [Required] string SourceLanguage);   // ví dụ "en"

public record ProjectResponse(
    Guid Id,
    string Name,
    string SourceLanguage,
    int PageCount,                       // 0 khi chưa upload slide
    DateTime CreatedAt);

public record SlidePageResponse(
    int PageNumber,                      // bắt đầu từ 1
    string ImageUrl,                     // đường dẫn ảnh trang slide
    string? NarrationText);              // văn bản thuyết minh gốc, null nếu chưa nhập

public record UpdateNarrationRequest(
    [Required] string NarrationText);

// Upload slide (POST /api/projects/{id}/slides/upload) dùng IFormFile ở Controller,
// không cần DTO riêng. Chỉ nhận .pdf, .png, .jpg.
