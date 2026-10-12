using System.ComponentModel.DataAnnotations;

namespace SlideNarrator.BLL.DTOs;

// ===== Nhóm Auth và Người dùng (US-01 đến US-04) =====

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required, MaxLength(100)] string FullName);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    string Role,          // "Admin" | "Presenter" | "Viewer"
    bool IsActive,
    DateTime CreatedAt);

public record LoginResponse(
    string AccessToken,
    string TokenType,     // luôn là "Bearer"
    int ExpiresInSeconds,
    UserResponse User);

public record UpdateRoleRequest(
    [Required] string Role);   // "Admin" | "Presenter" | "Viewer"

public record UpdateStatusRequest(
    bool IsActive);            // false = khóa tài khoản
