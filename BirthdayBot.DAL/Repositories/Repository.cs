using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BirthdayBot.DAL.Repositories
{
    public class Repository : IRepository
    {
        private readonly AppDbContext dbContext;

        public Repository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public T Add<T>(T exemplar)
            where T : class
        {
            T newExemplar = this.dbContext.Set<T>().Add(exemplar).Entity;
            this.dbContext.SaveChanges();
            return newExemplar;
        }

        public void AddRange<T>(IEnumerable<T> range)
            where T : class
        {
            this.dbContext.Set<T>().AddRange(range);
            this.dbContext.SaveChanges();
        }

        public void DeleteRange<T>(IEnumerable<T> range)
            where T : class
        {
            this.dbContext.Set<T>().RemoveRange(range);
            this.dbContext.SaveChanges();
        }

        public void Delete<T>(T exemplar)
            where T : class
        {
            this.dbContext.Set<T>().Remove(exemplar);
            this.dbContext.SaveChanges();
        }

        public void UpdateRange<T>(IEnumerable<T> exemplars)
            where T : class
        {
            this.dbContext.Set<T>().UpdateRange(exemplars);
            this.dbContext.SaveChanges();
        }

        public void Update<T>(T exemplar)
            where T : class
        {
            this.dbContext.Set<T>().Update(exemplar);
            this.dbContext.SaveChanges();
        }

        public void LoadReference<T, TProperty>(T exemplar, Expression<Func<T, TProperty>> expression) where T : class where TProperty : class
        {
            this.dbContext.Entry(exemplar).Reference(expression).Load();
        }

        public void LoadCollection<T, TProperty>(T exemplar, Expression<Func<T, IEnumerable<TProperty>>> expression) where T : class where TProperty : class
        {
            this.dbContext.Entry(exemplar).Collection(expression).Load();
        }

        public async Task<T> AddAsync<T>(T exemplar)
            where T : class
        {
            var newExemplarTask = await this.dbContext.Set<T>().AddAsync(exemplar);
            await this.dbContext.SaveChangesAsync();
            return newExemplarTask.Entity;
        }

        public async Task AddRangeAsync<T>(IEnumerable<T> range)
            where T : class
        {
            await this.dbContext.Set<T>().AddRangeAsync(range);
            await this.dbContext.SaveChangesAsync();
        }

        public Task DeleteRangeAsync<T>(IEnumerable<T> range)
            where T : class
        {
            this.dbContext.Set<T>().RemoveRange(range);
            return this.dbContext.SaveChangesAsync();
        }

        public Task DeleteAsync<T>(T exemplar)
            where T : class
        {
            this.dbContext.Set<T>().Remove(exemplar);
            return this.dbContext.SaveChangesAsync();
        }

        public Task UpdateAsync<T>(T exemplar)
            where T : class
        {
            this.dbContext.Set<T>().Update(exemplar);
            return this.dbContext.SaveChangesAsync();
        }

        public Task UpdateRangeAsync<T>(IEnumerable<T> range)
            where T : class
        {
            this.dbContext.Set<T>().UpdateRange(range);
            return this.dbContext.SaveChangesAsync();
        }

        public IEnumerable<T> GetRange<T>(bool tracking, Func<T, bool> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null) where T : class
        {
            IQueryable<T> query = this.dbContext.Set<T>();
            if (!tracking)
            {
                query = query.AsNoTracking();
            }
            if (include != null)
            {
                query = include(query);
            }

            return query.Where(predicate);
        }

        public T Get<T>(bool tracking, Func<T, bool> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null) where T : class
        {
            IQueryable<T> query = this.dbContext.Set<T>();
            if (!tracking)
            {
                query = query.AsNoTracking();
            }
            if (include != null)
            {
                query = include(query);
            }

            return query.FirstOrDefault(predicate);
        }

        public async Task<IEnumerable<T>> GetRangeAsync<T>(bool tracking, Func<T, bool> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null) where T : class
        {
            IQueryable<T> query = this.dbContext.Set<T>();
            if (!tracking)
            {
                query = query.AsNoTracking();
            }
            if (include != null)
            {
                query = include(query);
            }

            List<T> tList = await query.ToListAsync();
            return tList.Where(e => predicate(e));
        }

        public async Task<T> GetAsync<T>(bool tracking, Func<T, bool> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null) where T : class
        {
            IQueryable<T> query = this.dbContext.Set<T>();
            if (!tracking)
            {
                query = query.AsNoTracking();
            }
            if (include != null)
            {
                query = include(query);
            }

            List<T> tList = await query.ToListAsync();
            return tList.FirstOrDefault(predicate);
        }

        public async Task LoadReferenceAsync<T, TProperty>(T exemplar, Expression<Func<T, TProperty>> expression) where T : class where TProperty : class
        {
            await this.dbContext.Entry(exemplar).Reference(expression).LoadAsync();
        }

        public async Task LoadCollectionAsync<T, TProperty>(T exemplar, Expression<Func<T, IEnumerable<TProperty>>> expression) where T : class where TProperty : class
        {
            await this.dbContext.Entry(exemplar).Collection(expression).LoadAsync();
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }
    }
}
