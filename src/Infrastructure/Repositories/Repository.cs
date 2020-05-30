using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities.Base;
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

        /// <summary>
        /// Returns the first instance of the entity by id
        /// </summary>
        /// <param name="id">The entity id</param>
        /// <returns>The entity</returns>
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
            var entity = await GetAsync(id);
            if (entity != null)
            {
                Context.Instance.Set<TEntity>().Remove(entity);
                await SaveChangesAsync();
                return true;
            }
            // TODO add logging here
            return false;
        }

        /// <summary>
        /// Will remove an entity from the Database. It will check first and then remove.
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        /// <returns>If successful</returns>
        public async Task<bool> RemoveAsync(TEntity entity)
        {
            if (GetAsync(entity.Id) != null)
            {
                Context.Instance.Set<TEntity>().Remove(entity);
                await SaveChangesAsync();
                return true;
            }
            // TODO add logging here
            return false;
        }

        public async Task<bool> RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            Context.Instance.Set<TEntity>().RemoveRange(entities);
            await SaveChangesAsync();
            return true;
        }

        public virtual IQueryable<TEntity> BaseIncludes()
        {
            return Context.Instance.Set<TEntity>();
        }

        public Task<int> SaveChangesAsync()
        {
            return Context.SaveChangesAsync();
        }

    }
}
