using SlideNarrator.DAL.Entities;

namespace SlideNarrator.DAL.Repositories;

/// <summary>
/// Gom các repository và lưu thay đổi trong một giao dịch duy nhất.
/// Vòng đời của DbContext do DI (hoặc nơi tạo ra nó) quản lý, nên interface này không IDisposable.
/// </summary>
public interface IUnitOfWork
{
    IGenericRepository<Role> Roles { get; }
    IUserRepository Users { get; }
    IProjectRepository Projects { get; }
    ISlideRepository Slides { get; }
    ITranslationRepository Translations { get; }
    IAudioTrackRepository AudioTracks { get; }
    IJobRepository Jobs { get; }
    ILogRepository Logs { get; }
    IGenericRepository<ApiKeySetting> ApiKeys { get; }

    Task<int> SaveChangesAsync();

    Task ExecuteInTransactionAsync(Func<Task> work);
}
