using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities.Base;
using PlexRipper.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PlexRipper.Data.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly IPlexRipperDbContext Context;


        public Repository(IPlexRipperDbContext context)
        {
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

        public virtual async Task AddAsync(TEntity entity)
        {
            try
            {
                if (!IsTracking(entity))
                {
                    await Context.Instance.Set<TEntity>().AddAsync(entity);
                    await SaveChangesAsync();
                    await Context.Entry(entity).GetDatabaseValuesAsync();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"Exception occured during the adding of {typeof(TEntity)} to the Database");
                throw;
            }
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
            try
            {
                if (id <= 0)
                {
                    Log.Warning($"Id was {id}");
                }

                // Check if entity exists
                var entity = await GetAsync(id);
                if (entity != null)
                {
                    Context.Instance.Set<TEntity>().Remove(entity);
                    await SaveChangesAsync();
                    return true;
                }
                Log.Warning($"Entity with {id} of type {typeof(TEntity)} does not exist and can therefore not be removed.");
                return false;
            }
            catch (Exception e)
            {
                Log.Error(e, $"Exception occured during the removal of {typeof(TEntity)} from the Database");
                throw;
            }
        }

        /// <summary>
        /// Will remove an entity from the Database. It will check first and then remove.
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        /// <returns>If successful</returns>
        public async Task<bool> RemoveAsync(TEntity entity)
        {
            try
            {
                if (entity == null)
                {
                    Log.Warning($"Entity of type {typeof(TEntity)} to be removed was null");
                    return false;
                }

                if (GetAsync(entity.Id) != null)
                {
                    Context.Instance.Set<TEntity>().Remove(entity);
                    await SaveChangesAsync();
                    return true;
                }
                Log.Warning($"Entity with {entity.Id} of type {typeof(TEntity)} does not exist and can therefore not be removed.");
                return false;
            }
            catch (Exception e)
            {
                Log.Error(e, $"Exception occured during the removal of {typeof(TEntity)} from the Database");
                throw;
            }
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

        public async Task SaveChangesAsync()
        {
            try
            {
                Log.Verbose("Saving changes to database");
                await Context.SaveChangesAsync();
            }
            catch (SqliteException ex)
            {
                Log.Error(ex);
                throw;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Log.Warning($"There was a Concurrency Exception on an entity: {typeof(TEntity)}");
                // Source: https://docs.microsoft.com/en-us/ef/core/saving/concurrency
                foreach (var entry in ex.Entries)
                {
                    if (entry.Entity is TEntity)
                    {
                        var proposedValues = entry.CurrentValues;
                        var databaseValues = await entry.GetDatabaseValuesAsync();

                        foreach (var property in proposedValues.Properties)
                        {
                            var proposedValue = proposedValues[property];
                            var databaseValue = databaseValues[property];

                            // TODO: decide which value should be written to database
                            proposedValues[property] = proposedValue;
                        }

                        // Refresh original values to bypass next concurrency check
                        entry.OriginalValues.SetValues(databaseValues);
                    }
                    else
                    {
                        throw new NotSupportedException(
                            "Don't know how to handle concurrency conflicts for "
                            + entry.Metadata.Name);
                    }
                }
            }
        }

    }
}
