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
        Slides = new GenericRepository<SlidePage>(context);
        Translations = new GenericRepository<TranslationRecord>(context);
        AudioTracks = new GenericRepository<AudioTrack>(context);
        Jobs = new JobRepository(context);
        Logs = new GenericRepository<LogEntry>(context);
        ApiKeys = new GenericRepository<ApiKeySetting>(context);
    }

    public IGenericRepository<Role> Roles { get; }
    public IUserRepository Users { get; }
    public IProjectRepository Projects { get; }
    public IGenericRepository<SlidePage> Slides { get; }
    public IGenericRepository<TranslationRecord> Translations { get; }
    public IGenericRepository<AudioTrack> AudioTracks { get; }
    public IJobRepository Jobs { get; }
    public IGenericRepository<LogEntry> Logs { get; }
    public IGenericRepository<ApiKeySetting> ApiKeys { get; }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
