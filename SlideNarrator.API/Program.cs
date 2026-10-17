using Microsoft.EntityFrameworkCore;
using SlideNarrator.BLL;
using SlideNarrator.DAL.Data;
using SlideNarrator.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpClient<ITtsService, TtsService>();

var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? DesignTimeDbContextFactory.DefaultConnection;
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
