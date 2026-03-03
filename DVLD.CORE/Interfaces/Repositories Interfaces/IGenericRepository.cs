using DVLD.CORE.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DVLD.CORE.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<TResult?> GetProjectedByIdAsync<TResult>(
                Expression<Func<T, bool>> criteria,
                Expression<Func<T, TResult>> selector);
        Task<IEnumerable<T>> GetAllAsync(
            string[]? includes = null,
            bool tracked = true,
            Expression<Func<T, object>>? orderBy = null,
            EnOrderByDirection orderByDirection = EnOrderByDirection.Ascending,
            int? skip = null,
            int? take = null);
        Task<T?> FindAsync( Expression<Func<T, bool>> predicate, string[]? includes = null, bool tracked = true);
        Task<IEnumerable<T>> FindAllAsync(
            Expression<Func<T, bool>> predicate,
            string[]? includes = null,
            bool tracked = true,
            Expression<Func<T, object>>? orderBy = null,
            EnOrderByDirection orderByDirection = EnOrderByDirection.Ascending,
            int? skip = null,
            int? take = null);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Delete(T entity);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        void Attach(T entity);
        Task<bool> IsExistAsync(Expression<Func<T, bool>> predicate, string[]? includes = null);
    }
}
