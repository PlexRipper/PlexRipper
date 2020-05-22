using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        public ILogger Log { get; }
        protected readonly DbContext Context;

        public Repository(DbContext context, ILogger log)
        {
            Log = log;
            Context = context;

        }

        public bool IsTracking(TEntity entity)
        {
            return Context.ChangeTracker.Entries<TEntity>().Any(x => x.Entity.Id == entity.Id);
        }

        public async Task<TEntity> GetAsync(int id)
        {
            return await Context.Set<TEntity>()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public virtual async Task<TEntity> GetWithIncludeAsync(int id)
        {
            return await GetAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await Context.Set<TEntity>().ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllWithIncludeAsync()
        {
            return await GetAllAsync();
        }

        public async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Context.Set<TEntity>().Where(predicate).FirstOrDefaultAsync();
        }

        public virtual async Task<TEntity> FindWithIncludeAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await FindAsync(predicate);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Context.Set<TEntity>().Where(predicate).ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> FindAllWithIncludeAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await FindAllAsync(predicate);
        }

        public async Task<TEntity> SingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return await Context.Set<TEntity>().SingleOrDefaultAsync(predicate);
        }

        public async Task Add(TEntity entity)
        {
            await Context.Set<TEntity>().AddAsync(entity);
            await SaveChangesAsync();
            await Context.Entry(entity).GetDatabaseValuesAsync();
        }

        public async Task AddRange(IEnumerable<TEntity> entities)
        {
            var newEntities = entities.ToList();

            await Context.Set<TEntity>().AddRangeAsync(newEntities);
            await SaveChangesAsync();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            if (!IsTracking(entity))
            {
                Context.Set<TEntity>().Update(entity);
            }
            else
            {
                var exist = await Context.Set<TEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == entity.Id);
                Context.Entry(exist).CurrentValues.SetValues(entity);
            }
            await SaveChangesAsync();
        }

        public async Task<bool> RemoveAsync(int id)
        {
            var result = await GetAsync(id);
            if (result != null)
            {
                return await RemoveAsync(result);
            }
            return false;
            // TODO add logging here
        }

        public async Task<bool> RemoveAsync(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
            await SaveChangesAsync();
            return await GetAsync(entity.Id) == null;
        }

        public void RemoveRange(IEnumerable<int> ids)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().RemoveRange(entities);
        }

        public virtual IQueryable<TEntity> BaseIncludes()
        {
            return Context.Set<TEntity>();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }

    }
}
