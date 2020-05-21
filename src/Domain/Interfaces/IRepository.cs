using PlexRipper.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PlexRipper.Domain.Interfaces
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {
        public Task<TEntity> GetAsync(int id);
        public Task<TEntity> GetWithIncludeAsync(int id);


        public Task<IEnumerable<TEntity>> GetAllAsync();
        public Task<IEnumerable<TEntity>> GetAllWithIncludeAsync();

        public Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate);
        public Task<TEntity> FindWithIncludeAsync(Expression<Func<TEntity, bool>> predicate);
        public Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate);
        public Task<IEnumerable<TEntity>> FindAllWithIncludeAsync(Expression<Func<TEntity, bool>> predicate);

        public Task<TEntity> SingleOrDefault(Expression<Func<TEntity, bool>> predicate);

        public void Add(TEntity entity);
        public void AddRange(IEnumerable<TEntity> entities);

        public void Update(TEntity entity);

        public Task RemoveAsync(int id);
        public void Remove(TEntity entity);
        public void RemoveRange(IEnumerable<int> ids);
        public void RemoveRange(IEnumerable<TEntity> entities);
        public IQueryable<TEntity> BaseIncludes();
        public Task SaveChangesAsync();
    }
}
