using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SlideNarrator.DAL.Data;

/// <summary>
/// Giúp lệnh `dotnet ef migrations add ...` chạy được khi chưa có project API,
/// đồng thời là nơi quyết định chuỗi kết nối mặc định cho cả nhóm.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>Mặc định dùng MySQL (XAMPP / MariaDB cổng 3306).</summary>
    public const string DefaultConnection = "Server=localhost;Port=3306;Database=slidenarratordb;User=root;Password=;";

    /// <summary>
    /// Thứ tự ưu tiên: biến môi trường ConnectionStrings__Default,
    /// biến môi trường SLIDENARRATOR_CONNECTION (cách cũ), rồi mới tới chuỗi mặc định.
    /// </summary>
    public static string ResolveConnection() =>
        Environment.GetEnvironmentVariable("ConnectionStrings__Default")
        ?? Environment.GetEnvironmentVariable("SLIDENARRATOR_CONNECTION")
        ?? DefaultConnection;

    public AppDbContext CreateDbContext(string[] args)
    {
        var connStr = ResolveConnection();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(connStr, ServerVersion.AutoDetect(connStr))
            .Options;

        return new AppDbContext(options);
    }
}