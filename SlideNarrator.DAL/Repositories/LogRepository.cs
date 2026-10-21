using Microsoft.EntityFrameworkCore;
using SlideNarrator.DAL.Data;
using SlideNarrator.DAL.Entities;

namespace SlideNarrator.DAL.Repositories;

public class LogRepository : ILogRepository
{
    private readonly AppDbContext _context;

    public LogRepository(AppDbContext context) => _context = context;

    public async Task WriteAsync(string level, string action, string message, Guid? userId = null, string? errorCode = null)
    {
        await _context.LogEntries.AddAsync(new LogEntry
        {
            Level = level,
            Action = action.Length > 100 ? action[..100] : action,
            Message = message.Length > 2000 ? message[..2000] : message,
            UserId = userId,
            ErrorCode = errorCode
        });
    }

    public async Task<(List<LogEntry> Items, int Total)> GetPagedAsync(int page, int pageSize, string? level = null)
    {
        var query = _context.LogEntries.AsNoTracking();
        if (!string.IsNullOrEmpty(level)) query = query.Where(l => l.Level == level);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.Id)
                               .Skip((Math.Max(page, 1) - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();
        return (items, total);
    }
}
