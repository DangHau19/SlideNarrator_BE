using Microsoft.EntityFrameworkCore;
using SlideNarrator.BLL;
using SlideNarrator.DAL.Data;
using SlideNarrator.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký HttpClient và Service TTS
builder.Services.AddHttpClient<ITtsService, TtsService>();

// 2. Lấy chuỗi kết nối (kiểm tra key "DefaultConnection" hoặc "Default" trong appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? builder.Configuration.GetConnectionString("Default")
                       ?? DesignTimeDbContextFactory.DefaultConnection;

// 3. ĐĂNG KÝ DBCONTEXT SỬ DỤNG MYSQL
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

app.UseDefaultFiles();   // THÊM: vào "/" thì tự mở index.html
app.UseStaticFiles();    // THÊM: phục vụ file trong wwwroot

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();