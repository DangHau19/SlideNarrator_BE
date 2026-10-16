namespace SlideNarrator.DAL.Entities;

/// <summary>Tài khoản người dùng (khớp RegisterRequest / UserResponse của BLL).</summary>
public class UserAccount
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public ICollection<PresentationProject> Projects { get; set; } = new List<PresentationProject>();
    public ICollection<LogEntry> Logs { get; set; } = new List<LogEntry>();
}
