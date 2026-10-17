using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SlideNarrator.DAL.Data;

/// <summary>
/// Giúp lệnh `dotnet ef migrations add ...` chạy được khi chưa có project API,
/// đồng thời là nơi duy nhất quyết định chuỗi kết nối mặc định cho cả nhóm.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>Mặc định dùng SQL Server LocalDB (có sẵn khi cài Visual Studio).</summary>
    public const string DefaultConnection =
        "Server=(localdb)\\MSSQLLocalDB;Database=SlideNarratorDb;Trusted_Connection=True;TrustServerCertificate=True";

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
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ResolveConnection())
            .Options;
        return new AppDbContext(options);
    }
}
