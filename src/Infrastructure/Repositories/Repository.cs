using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Interfaces;
using PlexRipper.Infrastructure.Common.Interfaces;
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
        protected readonly IPlexRipperDbContext Context;
        public ILogger Log { get; }

        public Repository(IPlexRipperDbContext context, ILogger log)
        {
            Log = log;
            Context = context;
        }

        public bool IsTracking(TEntity entity)
        {
            return Context.ChangeTracker.Entries<TEntity>().Any(x => x.Entity.Id == entity.Id);
        }

        public Task<TEntity> GetAsync(int id)
        {
            return Context.Instance.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await Context.Instance.Set<TEntity>().ToListAsync();
        }

        public Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Instance.Set<TEntity>().Where(predicate).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Context.Instance.Set<TEntity>().Where(predicate).ToListAsync();
        }

        public Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Instance.Set<TEntity>().SingleOrDefaultAsync(predicate);
        }

        public async Task AddAsync(TEntity entity)
        {
            await Context.Instance.Set<TEntity>().AddAsync(entity);
            await SaveChangesAsync();
            await Context.Entry(entity).GetDatabaseValuesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            var newEntities = entities.ToList();

            await Context.Instance.Set<TEntity>().AddRangeAsync(newEntities);
            await SaveChangesAsync();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            if (!IsTracking(entity))
            {
                Context.Instance.Set<TEntity>().Update(entity);
            }
            else
            {
                var exist = await Context.Instance.Set<TEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == entity.Id);
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
            Context.Instance.Set<TEntity>().Remove(entity);
            await SaveChangesAsync();
            return await GetAsync(entity.Id) == null;
        }

        public void RemoveRange(IEnumerable<int> ids)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            Context.Instance.Set<TEntity>().RemoveRange(entities);
        }

        public virtual IQueryable<TEntity> BaseIncludes()
        {
            return Context.Instance.Set<TEntity>();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }

    }
}
