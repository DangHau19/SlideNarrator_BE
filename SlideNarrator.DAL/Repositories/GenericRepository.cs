using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SlideNarrator.DAL.Data;

namespace SlideNarrator.DAL.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    // protected để các repository chuyên biệt (kế thừa) dùng lại được
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> Set;

    public GenericRepository(AppDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id) => await Set.FindAsync(id);

    public async Task<List<T>> GetAllAsync() => await Set.AsNoTracking().ToListAsync();

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await Set.Where(predicate).ToListAsync();

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
        await Set.AnyAsync(predicate);

    public async Task AddAsync(T entity) => await Set.AddAsync(entity);

    public void Update(T entity) => Set.Update(entity);

    public void Remove(T entity) => Set.Remove(entity);
}
