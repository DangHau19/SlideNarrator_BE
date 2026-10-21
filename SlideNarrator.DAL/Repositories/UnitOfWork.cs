using SlideNarrator.DAL.Data;
using SlideNarrator.DAL.Entities;

namespace SlideNarrator.DAL.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Roles = new GenericRepository<Role>(context);
        Users = new UserRepository(context);
        Projects = new ProjectRepository(context);
        Slides = new SlideRepository(context);
        Translations = new TranslationRepository(context);
        AudioTracks = new AudioTrackRepository(context);
        Jobs = new JobRepository(context);
        Logs = new LogRepository(context);
        ApiKeys = new GenericRepository<ApiKeySetting>(context);
    }

    public IGenericRepository<Role> Roles { get; }
    public IUserRepository Users { get; }
    public IProjectRepository Projects { get; }
    public ISlideRepository Slides { get; }
    public ITranslationRepository Translations { get; }
    public IAudioTrackRepository AudioTracks { get; }
    public IJobRepository Jobs { get; }
    public ILogRepository Logs { get; }
    public IGenericRepository<ApiKeySetting> ApiKeys { get; }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    public async Task ExecuteInTransactionAsync(Func<Task> work)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await work();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
