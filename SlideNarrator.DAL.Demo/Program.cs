using Microsoft.EntityFrameworkCore;
using SlideNarrator.DAL.Data;
using SlideNarrator.DAL.Entities;
using SlideNarrator.DAL.Enums;
using SlideNarrator.DAL.Repositories;

// Kịch bản kiểm thử truy vấn dữ liệu (Data Unit Test) cho tầng DAL (báo cáo mục 5.4).
// Chạy trên MySQL thật qua DesignTimeDbContextFactory (cùng cấu hình với lệnh dotnet ef).
// Luồng dữ liệu kiểm thử: presentation_project -> slide_page -> translation_record -> audio_track,
// cộng thêm processing_job và log_entry. Cuối chương trình dọn dữ liệu test.

int passed = 0, failed = 0;
void Check(string id, string name, bool ok, string detail = "")
{
    if (ok) passed++; else failed++;
    Console.WriteLine($"[{(ok ? "PASS" : "FAIL")}] {id} - {name}{(detail.Length > 0 ? "  (" + detail + ")" : "")}");
}

// Mỗi lần gọi tạo một DbContext + UnitOfWork mới (giống mỗi HTTP request trong API)
(AppDbContext ctx, IUnitOfWork uow) NewScope()
{
    var ctx = new DesignTimeDbContextFactory().CreateDbContext(Array.Empty<string>());
    return (ctx, new UnitOfWork(ctx));
}

Console.WriteLine("== Chuẩn bị: áp dụng migration ==");
{
    var (ctx, _) = NewScope();
    await using (ctx) await ctx.Database.MigrateAsync();
}

Guid userId = Guid.Empty, projectId = Guid.Empty;
// Đặt biến môi trường SLIDENARRATOR_KEEP=1 để GIỮ dữ liệu test lại trong MySQL (chụp ảnh bảng cho báo cáo)
var keepData = Environment.GetEnvironmentVariable("SLIDENARRATOR_KEEP") == "1";
var stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

Console.WriteLine("== Chạy kịch bản ==");

// T01: tạo người dùng + dự án mới -> bảng presentation_project
{
    var (ctx, uow) = NewScope();
    await using (ctx)
    {
        var role = (await uow.Roles.FindAsync(r => r.Name == "Presenter")).Single();
        var user = new UserAccount { Email = $"test_{stamp}@example.com", FullName = "Nguoi Test", PasswordHash = "demo", RoleId = role.Id };
        await uow.Users.AddAsync(user);
        await uow.SaveChangesAsync();
        userId = user.Id;

        var project = new PresentationProject { Name = "Intro to SE", SourceLanguage = "en", SourceFileType = "pdf", UserAccountId = userId };
        await uow.Projects.AddAsync(project);
        await uow.SaveChangesAsync();
        projectId = project.Id;
        Check("T01", "Tạo dự án mới vào presentation_project", projectId != Guid.Empty && await uow.Projects.AnyAsync(p => p.Id == projectId));
    }
}

// T02: lưu 2 trang slide -> slide_page, PageCount cập nhật
{
    var (ctx, uow) = NewScope();
    await using (ctx)
    {
        await uow.Slides.AddRangeAsync(new[]
        {
            new SlidePage { PresentationProjectId = projectId, PageNumber = 1, ImagePath = "projects/p1/slide1.png", NarrationText = "Welcome to the course." },
            new SlidePage { PresentationProjectId = projectId, PageNumber = 2, ImagePath = "projects/p1/slide2.png", NarrationText = "Today we discuss layered architecture." }
        });
        var project = (await uow.Projects.GetByIdAsync(projectId))!;
        project.PageCount = 2;
        await uow.SaveChangesAsync();
        var count = await uow.Slides.CountAsync(s => s.PresentationProjectId == projectId);
        Check("T02", "Lưu 2 trang vào slide_page", count == 2, $"số trang = {count}");
    }
}

// T03: ràng buộc duy nhất (dự án, số trang) phải chặn trang trùng
{
    var (ctx, uow) = NewScope();
    await using (ctx)
    {
        await uow.Slides.AddAsync(new SlidePage { PresentationProjectId = projectId, PageNumber = 1, ImagePath = "dup.png" });
        var blocked = false;
        try { await uow.SaveChangesAsync(); } catch (DbUpdateException) { blocked = true; }
        Check("T03", "Chặn trang trùng số (unique PresentationProjectId + PageNumber)", blocked);
    }
}

// T04: lưu bản dịch vi + en cho trang 1; upsert cùng ngôn ngữ chỉ cập nhật, không thêm dòng mới
{
    var (ctx, uow) = NewScope();
    await using (ctx)
    {
        var slide1 = (await uow.Slides.GetByPageNumberAsync(projectId, 1))!;
        await uow.Translations.UpsertAsync(slide1.Id, "vi", "Chào mừng bạn đến với khóa học.");
        await uow.Translations.UpsertAsync(slide1.Id, "en", "Welcome to the course.");
        await uow.SaveChangesAsync();
        await uow.Translations.UpsertAsync(slide1.Id, "vi", "Chào mừng các bạn đến với khóa học.");   // sửa tay
        await uow.SaveChangesAsync();
        var total = await uow.Translations.CountAsync(t => t.SlidePageId == slide1.Id);
        var vi = (await uow.Translations.FindAsync(t => t.SlidePageId == slide1.Id && t.TargetLanguage == "vi")).Single();
        Check("T04", "Lưu bản dịch vào translation_record và upsert không nhân đôi",
              total == 2 && vi.TranslatedText.Contains("các bạn"), $"số bản dịch = {total}");
    }
}

// T05: lưu metadata audio -> audio_track
{
    var (ctx, uow) = NewScope();
    await using (ctx)
    {
        var slide1 = (await uow.Slides.GetByPageNumberAsync(projectId, 1))!;
        var vi = (await uow.Translations.FindAsync(t => t.SlidePageId == slide1.Id && t.TargetLanguage == "vi")).Single();
        await uow.AudioTracks.AddAsync(new AudioTrack
        {
            TranslationRecordId = vi.Id,
            FilePath = "projects/p1/slide1_vi.mp3",
            VoiceName = "vi-VN-HoaiMyNeural",
            VoiceGender = "Female",
            Speed = 1.0,
            DurationSeconds = 4.2
        });
        await uow.SaveChangesAsync();
        Check("T05", "Lưu metadata file âm thanh vào audio_track", await uow.AudioTracks.AnyAsync(a => a.TranslationRecordId == vi.Id));
    }
}

// T06: đọc liên kết 4 bảng: dự án -> trang -> bản dịch -> audio
{
    var (ctx, uow) = NewScope();
    await using (ctx)
    {
        var full = await uow.Projects.GetWithAudioAsync(projectId, "vi");
        var s1 = full?.Slides.FirstOrDefault(s => s.PageNumber == 1);
        var audio = s1?.Translations.SingleOrDefault()?.AudioTracks.SingleOrDefault();
        Check("T06", "Đọc dự án kèm trang, bản dịch, audio (lọc ngôn ngữ vi)",
              full is not null && full.Slides.Count == 2 && audio?.VoiceName == "vi-VN-HoaiMyNeural",
              $"{full?.Name} | trang {s1?.PageNumber} | {s1?.Translations.Count} bản dịch | audio {audio?.FilePath}");
        Console.WriteLine("       Cây dữ liệu:");
        foreach (var s in full!.Slides)
        {
            Console.WriteLine($"       - Trang {s.PageNumber}: {s.NarrationText}");
            foreach (var t in s.Translations)
            {
                Console.WriteLine($"           [{t.TargetLanguage}] {t.TranslatedText}");
                foreach (var a in t.AudioTracks) Console.WriteLine($"               audio: {a.FilePath} ({a.VoiceName}, {a.Speed}x)");
            }
        }
        var byAudio = await uow.AudioTracks.GetByProjectAsync(projectId, "vi");
        Check("T07", "Lấy toàn bộ audio của dự án theo ngôn ngữ", byAudio.Count == 1, $"số audio = {byAudio.Count}");
    }
}

// T08: danh sách dự án của người dùng
{
    var (ctx, uow) = NewScope();
    await using (ctx)
    {
        var list = await uow.Projects.GetByOwnerAsync(userId);
        Check("T08", "Lấy danh sách dự án của người dùng", list.Count == 1 && list[0].Id == projectId);
    }
}

// T09: giao dịch COMMIT: trang 3 + bản dịch + audio được lưu cùng lúc
{
    var (ctx, uow) = NewScope();
    await using (ctx)
    {
        await uow.ExecuteInTransactionAsync(async () =>
        {
            var slide = new SlidePage { PresentationProjectId = projectId, PageNumber = 3, ImagePath = "projects/p1/slide3.png", NarrationText = "Q&A" };
            await uow.Slides.AddAsync(slide);
            await uow.SaveChangesAsync();
            var tr = await uow.Translations.UpsertAsync(slide.Id, "vi", "Hỏi đáp");
            await uow.SaveChangesAsync();
            await uow.AudioTracks.AddAsync(new AudioTrack { TranslationRecordId = tr.Id, FilePath = "projects/p1/slide3_vi.mp3", VoiceName = "vi-VN-HoaiMyNeural", VoiceGender = "Female" });
            await uow.SaveChangesAsync();
        });
    }
    var (ctx2, uow2) = NewScope();
    await using (ctx2)
    {
        var s3 = await uow2.Slides.GetByPageNumberAsync(projectId, 3);
        var ok = s3 is not null
                 && await uow2.Translations.AnyAsync(t => t.SlidePageId == s3.Id)
                 && (await uow2.AudioTracks.GetByProjectAsync(projectId, "vi")).Any(a => a.FilePath.EndsWith("slide3_vi.mp3"));
        Check("T09", "Giao dịch COMMIT: lưu đồng thời slide + bản dịch + audio", ok);
    }
}

// T10: giao dịch ROLLBACK: lỗi ở bước cuối thì trang 4 + bản dịch phải biến mất
{
    var (ctx, uow) = NewScope();
    await using (ctx)
    {
        var failedAsExpected = false;
        try
        {
            await uow.ExecuteInTransactionAsync(async () =>
            {
                var slide = new SlidePage { PresentationProjectId = projectId, PageNumber = 4, ImagePath = "projects/p1/slide4.png" };
                await uow.Slides.AddAsync(slide);
                await uow.SaveChangesAsync();
                await uow.Translations.UpsertAsync(slide.Id, "vi", "Trang này sẽ bị hủy");
                await uow.SaveChangesAsync();
                throw new InvalidOperationException("Giả lập lỗi khi tạo audio");
            });
        }
        catch (InvalidOperationException) { failedAsExpected = true; }
        ctx.ChangeTracker.Clear();
        var s4 = await uow.Slides.GetByPageNumberAsync(projectId, 4);
        Check("T10", "Giao dịch ROLLBACK: lỗi giữa chừng thì không lưu gì", failedAsExpected && s4 is null);
    }
}

// T11: tiến trình job + log_entry
{
    var (ctx, uow) = NewScope();
    await using (ctx)
    {
        var job = new ProcessingJob { PresentationProjectId = projectId, Type = JobType.Translate, TargetLanguage = "vi" };
        await uow.Jobs.AddAsync(job);
        await uow.SaveChangesAsync();

        uow.Jobs.Start(job, "Đang dịch");
        await uow.Logs.WriteAsync("Information", "JobStart", $"Job {job.Id} bắt đầu", userId);
        await uow.SaveChangesAsync();
        uow.Jobs.ReportProgress(job, 60, "Đang tạo giọng đọc");
        await uow.SaveChangesAsync();
        uow.Jobs.Complete(job, "projects/p1/output_vi.zip");
        await uow.Logs.WriteAsync("Information", "JobDone", $"Job {job.Id} hoàn tất", userId);
        await uow.SaveChangesAsync();

        var reload = (await uow.Jobs.GetByProjectAsync(projectId)).Single();
        var (logs, total) = await uow.Logs.GetPagedAsync(1, 10, "Information");
        Check("T11", "Ghi tiến trình vào processing_job và log_entry",
              reload.Status == JobStatus.Succeeded && reload.Progress == 100 && reload.FinishedAt is not null && total >= 2,
              $"job {reload.Status} {reload.Progress}%, log Information = {total}");
    }
}

// T12: xóa dự án -> cascade xóa mọi bảng con
if (keepData)
{
    Console.WriteLine("[SKIP] T12 - Bỏ qua xóa dự án vì SLIDENARRATOR_KEEP=1 (dữ liệu test được giữ lại)");
}
else
{
    var (ctx, uow) = NewScope();
    await using (ctx)
    {
        var project = (await uow.Projects.GetByIdAsync(projectId))!;
        uow.Projects.Remove(project);
        await uow.SaveChangesAsync();
        var left = await uow.Slides.CountAsync(s => s.PresentationProjectId == projectId)
                 + await uow.Jobs.CountAsync(j => j.PresentationProjectId == projectId);
        Check("T12", "Xóa dự án kéo theo xóa slide, bản dịch, audio, job (cascade)", left == 0 && !await uow.Projects.AnyAsync(p => p.Id == projectId));
    }
}

// Dọn người dùng test
if (!keepData)
{
    var (ctx, uow) = NewScope();
    await using (ctx)
    {
        var user = await uow.Users.GetByIdAsync(userId);
        if (user is not null) { uow.Users.Remove(user); await uow.SaveChangesAsync(); }
    }
}

Console.WriteLine($"== Kết quả: {passed} PASS, {failed} FAIL ==");
return failed == 0 ? 0 : 1;
