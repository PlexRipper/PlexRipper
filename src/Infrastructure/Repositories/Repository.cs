using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

        public async Task<TEntity> Add(TEntity entity)
        {
            await Context.Set<TEntity>().AddAsync(entity);
            await SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<TEntity>> AddRange(IEnumerable<TEntity> entities)
        {
            await Context.Set<TEntity>().AddRangeAsync(entities);
            await SaveChangesAsync();
            return entities;
        }

        public async Task<int> UpdateAsync(
            TEntity entity,
            params Expression<Func<TEntity, object>>[] navigations)
        {
            // From: https://stackoverflow.com/a/55190929
            var dbEntity = GetWithIncludeAsync(entity.Id);

            var dbEntry = Context.Entry(dbEntity);
            dbEntry.CurrentValues.SetValues(entity);

            foreach (var property in navigations)
            {
                var propertyName = property.GetPropertyAccess().Name;
                var dbItemsEntry = dbEntry.Collection(propertyName);
                var accessor = dbItemsEntry.Metadata.GetCollectionAccessor();

                await dbItemsEntry.LoadAsync();
                var dbItemsMap = ((IEnumerable<BaseEntity>)dbItemsEntry.CurrentValue)
                    .ToDictionary(e => e.Id);

                var items = (IEnumerable<BaseEntity>)accessor.GetOrCreate(entity, false);

                foreach (var item in items)
                {
                    if (!dbItemsMap.TryGetValue(item.Id, out var oldItem))
                        accessor.Add(dbEntity, item, false);
                    else
                    {
                        Context.Entry(oldItem).CurrentValues.SetValues(item);
                        dbItemsMap.Remove(item.Id);
                    }
                }

                foreach (var oldItem in dbItemsMap.Values)
                    accessor.Remove(dbEntity, oldItem);
            }

            return await SaveChangesAsync();
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
            return Context.Set<TEntity>().AsNoTracking();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }

    }
}
