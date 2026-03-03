using DVLD.CORE.Enums;
using DVLD.CORE.Interfaces;
using DVLD.INFRASTRUCTURE.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DVLD.INFRASTRUCTURE.Repositories
{
    public class GenericRepository<T>(AppDbContext context) : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext _context = context;

        public async Task<T?> GetByIdAsync(int id) => await _context.Set<T>().FindAsync(id);
        public async Task<TResult?> GetProjectedByIdAsync<TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> selector)
        {
            return await _context.Set<T>()
                .Where(predicate)
                .Select(selector)
                .FirstOrDefaultAsync();
        }

        public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate, string[]? includes = null, bool tracked = true)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            if (!tracked)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            string[]? includes = null,
            bool tracked = true,
            Expression<Func<T, object>>? orderBy = null,
            EnOrderByDirection orderByDirection = EnOrderByDirection.Ascending,
            int? skip = null,
            int? take = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (!tracked)
                query = query.AsNoTracking();

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            if (orderBy != null)
                if (orderByDirection == EnOrderByDirection.Ascending)
                    query = query.OrderBy(orderBy);
                else
                    query = query.OrderByDescending(orderBy);


            if (skip.HasValue)
                query = query.Skip(skip.Value);
            if (take.HasValue)
                query = query.Take(take.Value);


            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAllAsync(
            Expression<Func<T, bool>> predicate,
            string[]? includes = null,
            bool tracked = true,
            Expression<Func<T, object>>? orderBy = null,
            EnOrderByDirection orderByDirection = EnOrderByDirection.Ascending,
            int? skip = null,
            int? take = null)
        {
            IQueryable<T> query = _context.Set<T>().Where(predicate);

            if (!tracked)
                query = query.AsNoTracking();

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            if (orderBy != null)
                if (orderByDirection == EnOrderByDirection.Ascending)
                    query = query.OrderBy(orderBy);
                else
                    query = query.OrderByDescending(orderBy);

            if (skip.HasValue)
                query = query.Skip(skip.Value);
            if (take.HasValue)
                query = query.Take(take.Value);


            return await query.ToListAsync();
        }

        public async Task AddAsync(T entity) => await _context.Set<T>().AddAsync(entity);

        public async Task AddRangeAsync(IEnumerable<T> entities) => await _context.Set<T>().AddRangeAsync(entities);

        public void Delete(T entity) => _context.Set<T>().Remove(entity);

        public void Update(T entity) => _context.Set<T>().Update(entity);

        public void Attach(T entity) 
        {
            _context.Set<T>().Attach(entity);
        }
        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate != null)
                return await _context.Set<T>().CountAsync(predicate);

            return await _context.Set<T>().CountAsync();
        }
        public async Task<bool> IsExistAsync(Expression<Func<T, bool>> predicate, string[]? includes = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            return await query.AnyAsync(predicate);
        }
    }
}
