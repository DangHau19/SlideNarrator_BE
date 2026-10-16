using SlideNarrator.DAL.Entities;

namespace SlideNarrator.DAL.Repositories;

public interface IUserRepository : IGenericRepository<UserAccount>
{
    /// <summary>Tìm tài khoản theo email (kèm Role). Dùng cho đăng ký / kiểm tra trùng.</summary>
    Task<UserAccount?> GetByEmailAsync(string email);
}
