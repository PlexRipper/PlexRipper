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
        public IQueryable<TEntity> BaseIncludes();
        #region Create
        public Task Add(TEntity entity);

        public Task AddRange(IEnumerable<TEntity> entities);




        #endregion

        #region Update
        public Task UpdateAsync(TEntity entity);




        #endregion

        #region Read
        public Task<TEntity> GetAsync(int id);
        public Task<TEntity> GetWithIncludeAsync(int id);


        public Task<IEnumerable<TEntity>> GetAllAsync();
        public Task<IEnumerable<TEntity>> GetAllWithIncludeAsync();

        public Task<TEntity> SingleOrDefault(Expression<Func<TEntity, bool>> predicate);

        public Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate);
        public Task<TEntity> FindWithIncludeAsync(Expression<Func<TEntity, bool>> predicate);
        public Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate);
        public Task<IEnumerable<TEntity>> FindAllWithIncludeAsync(Expression<Func<TEntity, bool>> predicate);
        #endregion

        #region Delete


        public Task<bool> RemoveAsync(int id);

        public Task<bool> RemoveAsync(TEntity entity);

        public void RemoveRange(IEnumerable<int> ids);

        public void RemoveRange(IEnumerable<TEntity> entities);
        #endregion

        public Task<int> SaveChangesAsync();
    }
}
