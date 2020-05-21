using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly DbContext Context;

        public Repository(DbContext context)
        {
            Context = context;
        }

        public async Task<TEntity> GetAsync(int id)
        {
            return await Context.Set<TEntity>().FindAsync(id);
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

        public void Add(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().AddRange(entities);
        }

        public void Update(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveAsync(int id)
        {
            var result = await GetAsync(id);
            if (result != null)
            {
                Remove(result);
            }
            // TODO add logging here
        }

        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
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
            return Context.Set<TEntity>().AsNoTracking();
        }

        public async Task SaveChangesAsync()
        {
            await Context.SaveChangesAsync();
        }

    }
}
