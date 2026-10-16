using Microsoft.EntityFrameworkCore;
using SlideNarrator.DAL.Entities;

namespace SlideNarrator.DAL.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<PresentationProject> PresentationProjects => Set<PresentationProject>();
    public DbSet<SlidePage> SlidePages => Set<SlidePage>();
    public DbSet<TranslationRecord> TranslationRecords => Set<TranslationRecord>();
    public DbSet<AudioTrack> AudioTracks => Set<AudioTrack>();
    public DbSet<ProcessingJob> ProcessingJobs => Set<ProcessingJob>();
    public DbSet<LogEntry> LogEntries => Set<LogEntry>();
    public DbSet<ApiKeySetting> ApiKeySettings => Set<ApiKeySetting>();

    // Id cố định cho dữ liệu khởi tạo (seed), để migration ổn định giữa các máy
    public static readonly Guid AdminRoleId = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid PresenterRoleId = new("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ViewerRoleId = new("33333333-3333-3333-3333-333333333333");

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Role>(e =>
        {
            e.ToTable("ROLE");
            e.Property(x => x.Name).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
            e.HasData(
                new Role { Id = AdminRoleId, Name = "Admin" },
                new Role { Id = PresenterRoleId, Name = "Presenter" },
                new Role { Id = ViewerRoleId, Name = "Viewer" });
        });

        b.Entity<UserAccount>(e =>
        {
            e.ToTable("USER_ACCOUNT");
            e.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            // 1 Role - N UserAccount
            e.HasOne(x => x.Role).WithMany(r => r.Users)
                .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<PresentationProject>(e =>
        {
            e.ToTable("PRESENTATION_PROJECT");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.SourceLanguage).HasMaxLength(10).IsRequired();
            e.Property(x => x.SourceFileType).HasMaxLength(20).IsRequired();
            e.Property(x => x.OriginalFilePath).HasMaxLength(500);
            // 1 UserAccount - N PresentationProject
            e.HasOne(x => x.Owner).WithMany(u => u.Projects)
                .HasForeignKey(x => x.UserAccountId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<SlidePage>(e =>
        {
            e.ToTable("SLIDE_PAGE");
            e.Property(x => x.ImagePath).HasMaxLength(500).IsRequired();
            e.Property(x => x.NarrationText);
            e.HasIndex(x => new { x.PresentationProjectId, x.PageNumber }).IsUnique();
            // 1 PresentationProject - N SlidePage
            e.HasOne(x => x.Project).WithMany(p => p.Slides)
                .HasForeignKey(x => x.PresentationProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<TranslationRecord>(e =>
        {
            e.ToTable("TRANSLATION_RECORD");
            e.Property(x => x.TargetLanguage).HasMaxLength(10).IsRequired();
            e.HasIndex(x => new { x.SlidePageId, x.TargetLanguage }).IsUnique();
            // 1 SlidePage - N TranslationRecord (mỗi ngôn ngữ đích một bản dịch)
            e.HasOne(x => x.SlidePage).WithMany(s => s.Translations)
                .HasForeignKey(x => x.SlidePageId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<AudioTrack>(e =>
        {
            e.ToTable("AUDIO_TRACK");
            e.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
            e.Property(x => x.VoiceName).HasMaxLength(100).IsRequired();
            e.Property(x => x.VoiceGender).HasMaxLength(10).IsRequired();
            // 1 TranslationRecord - N AudioTrack (nhiều giọng đọc / tốc độ)
            e.HasOne(x => x.TranslationRecord).WithMany(t => t.AudioTracks)
                .HasForeignKey(x => x.TranslationRecordId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ProcessingJob>(e =>
        {
            e.ToTable("PROCESSING_JOB");
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TargetLanguage).HasMaxLength(10);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.CurrentStep).HasMaxLength(50);
            e.Property(x => x.OutputFilePath).HasMaxLength(500);
            e.Property(x => x.ErrorMessage).HasMaxLength(2000);
            // Chỉ mục giúp tác vụ nền truy vấn nhanh các job theo trạng thái (ví dụ: Pending)
            e.HasIndex(x => x.Status);
            // 1 PresentationProject - N ProcessingJob
            e.HasOne(x => x.Project).WithMany(p => p.Jobs)
                .HasForeignKey(x => x.PresentationProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<LogEntry>(e =>
        {
            e.ToTable("LOG_ENTRY");
            e.Property(x => x.Level).HasMaxLength(20).IsRequired();
            e.Property(x => x.Action).HasMaxLength(100).IsRequired();
            e.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            e.Property(x => x.ErrorCode).HasMaxLength(50);
            e.HasIndex(x => x.Timestamp);
            // 1 UserAccount - N LogEntry (UserId có thể null: log hệ thống)
            e.HasOne(x => x.User).WithMany(u => u.Logs)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<ApiKeySetting>(e =>
        {
            e.ToTable("API_KEY_SETTING");
            e.Property(x => x.Provider).HasMaxLength(50).IsRequired();
            e.Property(x => x.EncryptedKey).HasMaxLength(1000).IsRequired();
            e.HasIndex(x => x.Provider).IsUnique();
        });
    }
}
