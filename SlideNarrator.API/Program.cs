using SlideNarrator.BLL;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký HttpClient và Service TTS cho hệ thống
builder.Services.AddHttpClient<ITtsService, TtsService>();

// Quản lý dự án bài giảng: bản lưu trong bộ nhớ, thay bằng bản dùng DAL khi TV2 xong
builder.Services.AddSingleton<IProjectService, InMemoryProjectService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
