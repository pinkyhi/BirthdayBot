using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BirthdayBot.DAL.Interfaces
{
    public interface IRepository : IDisposable
    {
        IEnumerable<T> GetRange<T>(bool tracking, Func<T, bool> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
            where T : class;

        T Get<T>(bool tracking, Func<T, bool> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
            where T : class;

        T Add<T>(T exemplar)
            where T : class;

        void AddRange<T>(IEnumerable<T> range)
            where T : class;

        void DeleteRange<T>(IEnumerable<T> range)
            where T : class;

        void Delete<T>(T exemplar)
            where T : class;

        void Update<T>(T exemplar)
            where T : class;

        void UpdateRange<T>(IEnumerable<T> range)
            where T : class;

        Task<IEnumerable<T>> GetRangeAsync<T>(bool tracking, Func<T, bool> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
            where T : class;

        Task<T> GetAsync<T>(bool tracking, Func<T, bool> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
            where T : class;

        Task<T> AddAsync<T>(T exemplar)
            where T : class;

        Task AddRangeAsync<T>(IEnumerable<T> range)
            where T : class;

        Task DeleteRangeAsync<T>(IEnumerable<T> range)
            where T : class;

        Task DeleteAsync<T>(T exemplar)
            where T : class;

        Task UpdateAsync<T>(T exemplar)
            where T : class;

        Task UpdateRangeAsync<T>(IEnumerable<T> exemplars)
            where T : class;
    }
}
