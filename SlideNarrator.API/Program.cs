using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SlideNarrator.BLL;
using SlideNarrator.DAL.Data;
using SlideNarrator.DAL.Repositories;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký HttpClient cho Service TTS và Dịch thuật
builder.Services.AddHttpClient<ITtsService, TtsService>();
builder.Services.AddHttpClient<ITranslationService, TranslationService>();

// 2. Lấy chuỗi kết nối
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? builder.Configuration.GetConnectionString("Default")
                       ?? DesignTimeDbContextFactory.DefaultConnection;

// 3. Đăng ký DbContext cho MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 4. Đăng ký các tầng Repositories & Services của nhóm
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5. Cấu hình môi trường Development & Tự động chạy Migration
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    try
    {
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Lỗi Migration/Kết nối MySQL]: {ex.Message}");
    }
}

// 6. Comment dòng này lại để tránh lỗi chuyển hướng khi chạy http://localhost:5000
// app.UseHttpsRedirection();

// 7. CẤU HÌNH GIAO DIỆN WEB CỦA THÀNH VIÊN 1 (WWWROOT)
app.UseDefaultFiles();   // Vào "/" thì tự động mở index.html
app.UseStaticFiles();    // Phục vụ file HTML, CSS, JS trong wwwroot

// 8. CẤU HÌNH PHỤC VỤ FILE ÂM THANH CHO TRÌNH DUYỆT PHÁT NHẠC (/audio/...)
var outputAudioDir = Path.Combine(Directory.GetCurrentDirectory(), "OutputAudio");
if (!Directory.Exists(outputAudioDir))
{
    Directory.CreateDirectory(outputAudioDir);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(outputAudioDir),
    RequestPath = "/audio"
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();