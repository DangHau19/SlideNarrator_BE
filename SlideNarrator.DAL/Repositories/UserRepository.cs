using Microsoft.EntityFrameworkCore;
using SlideNarrator.DAL.Data;
using SlideNarrator.DAL.Entities;

namespace SlideNarrator.DAL.Repositories;

public class UserRepository : GenericRepository<UserAccount>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public Task<UserAccount?> GetByEmailAsync(string email) =>
        Set.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);
}
