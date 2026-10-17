using Microsoft.EntityFrameworkCore;
using SlideNarrator.DAL.Data;
using SlideNarrator.DAL.Entities;
using SlideNarrator.DAL.Enums;
using SlideNarrator.DAL.Repositories;

// Demo tầng DAL: tạo DB bằng migration, lưu thử 1 dự án + 2 slide + bản dịch + audio + job,
// rồi đọc lại. Chụp màn hình output này để đưa vào báo cáo tiến độ (mục 4.1).

var connection = DesignTimeDbContextFactory.ResolveConnection();

var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connection).Options;
await using var context = new AppDbContext(options);

Console.WriteLine("1) Áp dụng migration...");
await context.Database.MigrateAsync();

IUnitOfWork uow = new UnitOfWork(context);

var presenterRole = (await uow.Roles.FindAsync(r => r.Name == "Presenter")).Single();

var stamp = DateTime.UtcNow.ToString("HHmmss");
var user = new UserAccount
{
    FullName = $"Demo {stamp}",
    Email = $"demo_{stamp}@example.com",
    PasswordHash = "chua-ma-hoa-chi-de-demo",
    RoleId = presenterRole.Id
};
await uow.Users.AddAsync(user);
await uow.SaveChangesAsync();

Console.WriteLine("2) Lưu dự án slide + 2 trang + bản dịch + audio + job...");
var project = new PresentationProject
{
    Name = "Intro to Software Engineering",
    SourceLanguage = "en",
    SourceFileType = "pdf",
    PageCount = 2,
    UserAccountId = user.Id,
    Slides =
    {
        new SlidePage { PageNumber = 1, ImagePath = "storage/p1/slide1.png", NarrationText = "Welcome to the course." },
        new SlidePage { PageNumber = 2, ImagePath = "storage/p1/slide2.png", NarrationText = "Today we discuss layered architecture." }
    }
};
await uow.Projects.AddAsync(project);
await uow.SaveChangesAsync();

var firstSlide = project.Slides.First();
var translation = new TranslationRecord
{
    SlidePageId = firstSlide.Id,
    TargetLanguage = "vi",
    TranslatedText = "Chào mừng bạn đến với khóa học."
};
await uow.Translations.AddAsync(translation);
await uow.SaveChangesAsync();

await uow.AudioTracks.AddAsync(new AudioTrack
{
    TranslationRecordId = translation.Id,
    FilePath = "storage/p1/slide1_vi.mp3",
    VoiceName = "vi-VN-HoaiMyNeural",
    VoiceGender = "Female",
    Speed = 1.0
});
await uow.Jobs.AddAsync(new ProcessingJob
{
    PresentationProjectId = project.Id,
    Type = JobType.Translate,
    TargetLanguage = "vi",
    Status = JobStatus.Pending,
    Progress = 0,
    CurrentStep = "Chờ xử lý"
});
await uow.SaveChangesAsync();

Console.WriteLine("3) Đọc lại từ database:");
var loaded = await context.PresentationProjects
    .Include(p => p.Slides).ThenInclude(s => s.Translations).ThenInclude(t => t.AudioTracks)
    .Include(p => p.Jobs)
    .FirstAsync(p => p.Id == project.Id);

Console.WriteLine($"Project #{loaded.Id}: {loaded.Name} ({loaded.SourceLanguage}, {loaded.SourceFileType}, {loaded.PageCount} trang)");
foreach (var slide in loaded.Slides.OrderBy(s => s.PageNumber))
{
    Console.WriteLine($"  Slide {slide.PageNumber}: {slide.NarrationText}");
    foreach (var t in slide.Translations)
    {
        Console.WriteLine($"    [{t.TargetLanguage}] {t.TranslatedText}");
        foreach (var a in t.AudioTracks)
            Console.WriteLine($"      audio: {a.FilePath} ({a.VoiceName})");
    }
}
foreach (var job in loaded.Jobs)
    Console.WriteLine($"  Job {job.Id} -> {job.TargetLanguage}: {job.Status} ({job.Progress}%)");

Console.WriteLine("Hoàn tất.");
