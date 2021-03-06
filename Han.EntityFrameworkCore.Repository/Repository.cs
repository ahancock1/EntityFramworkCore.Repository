﻿namespace Han.EntityFrameworkCore.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    ///     Creates an instance of a generic repository for a <see cref="DbContext" /> and
    ///     exposes basic CRUD functionality
    /// </summary>
    /// <typeparam name="TContext">The data context used for this repository</typeparam>
    /// <typeparam name="TEntity">The entity type used for this repository</typeparam>
    public abstract class Repository<TContext, TEntity> : IRepository<TEntity>
        where TContext : DbContext
        where TEntity : class
    {
        /// <inheritdoc cref="IRepository{TEntity}.All"/>
        public IEnumerable<TEntity> All(
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> sort = null,
            int? skip = null,
            int? take = null,
            params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
        {
            return Query(predicate, sort, skip, take, includes);
        }

        /// <inheritdoc cref="IRepository{TEntity}.AllAsync"/>
        public async Task<IEnumerable<TEntity>> AllAsync(
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> sort = null,
            int? skip = null,
            int? take = null,
            params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
        {
            return await QueryAsync(predicate, sort, skip, take, includes);
        }

        /// <inheritdoc cref="IRepository{TEntity}.AnyAsync"/>
        public async Task<bool> AnyAsync(
            Expression<Func<TEntity, bool>> predicate = null,
            params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
        {
            using (var context = GetDataContext())
            {
                var entities = AggregateIncludes(context, includes);

                return await (predicate != null ? entities.AnyAsync(predicate) : entities.AnyAsync());
            }
        }

        /// <inheritdoc cref="IRepository{TEntity}.Create"/>
        public bool Create(params TEntity[] entities)
        {
            using (var context = GetDataContext())
            {
                var set = context.Set<TEntity>();

                set.AddRange(entities);

                return context.SaveChanges() >= entities.Length;
            }
        }

        /// <inheritdoc cref="IRepository{TEntity}.CreateAsync"/>
        public async Task<bool> CreateAsync(params TEntity[] entities)
        {
            using (var context = GetDataContext())
            {
                var set = context.Set<TEntity>();

                await set.AddRangeAsync(entities);

                return await context.SaveChangesAsync() >= entities.Length;
            }
        }

        /// <inheritdoc cref="IRepository{TEntity}.Delete"/>
        public bool Delete(params TEntity[] entities)
        {
            using (var context = GetDataContext())
            {
                var set = context.Set<TEntity>();

                set.RemoveRange(entities);

                return context.SaveChanges() >= entities.Length;
            }
        }

        /// <inheritdoc cref="IRepository{TEntity}.DeleteAsync"/>
        public async Task<bool> DeleteAsync(params TEntity[] entities)
        {
            using (var context = GetDataContext())
            {
                var set = context.Set<TEntity>();

                set.RemoveRange(entities);

                return await context.SaveChangesAsync() >= entities.Length;
            }
        }

        /// <inheritdoc cref="IRepository{TEntity}.Update"/>
        public bool Update(params TEntity[] entities)
        {
            using (var context = GetDataContext())
            {
                var set = context.Set<TEntity>();

                set.UpdateRange(entities);

                return context.SaveChanges() >= entities.Length;
            }
        }

        /// <inheritdoc cref="IRepository{TEntity}.UpdateAsync"/>
        public async Task<bool> UpdateAsync(params TEntity[] entities)
        {
            using (var context = GetDataContext())
            {
                var set = context.Set<TEntity>();

                set.UpdateRange(entities);

                return await context.SaveChangesAsync() >= entities.Length;
            }
        }

        /// <inheritdoc cref="IRepository{TEntity}.Any"/>
        public bool Any(
            Expression<Func<TEntity, bool>> predicate,
            params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
        {
            using (var context = GetDataContext())
            {
                var entities = AggregateIncludes(context, includes);

                return predicate != null ? entities.Any(predicate) : entities.Any();
            }
        }

        /// <inheritdoc cref="IRepository{TEntity}.Get"/>
        public TEntity Get(
            Expression<Func<TEntity, bool>> predicate,
            params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
        {
            using (var context = GetDataContext())
            {
                return AggregateIncludes(context, includes).FirstOrDefault(predicate);
            }
        }

        /// <inheritdoc cref="IRepository{TEntity}.GetAsync"/>
        public async Task<TEntity> GetAsync(
            Expression<Func<TEntity, bool>> predicate,
            params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
        {
            using (var context = GetDataContext())
            {
                return await AggregateIncludes(context, includes).FirstOrDefaultAsync(predicate);
            }
        }

        private IQueryable<TEntity> AggregateIncludes(TContext context,
            params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
        {
            return includes.Aggregate((IQueryable<TEntity>)context.Set<TEntity>(),
                (current, include) => include(current)).AsQueryable();
        }

        private IQueryable<TEntity> AggregateQuery(
            TContext context,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> sort = null,
            int? skip = null,
            int? take = null,
            params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
        {
            var items = AggregateIncludes(context, includes);

            if (predicate != null)
            {
                items = items.Where(predicate);
            }

            if (sort != null)
            {
                items = sort(items);
            }

            if (skip.HasValue)
            {
                items = items.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                items = items.Take(take.Value);
            }

            return items;
        }

        private Task<List<TEntity>> QueryAsync(
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> sort = null,
            int? skip = null,
            int? take = null,
            params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
        {
            using (var context = GetDataContext())
            {
                return AggregateQuery(context, predicate, sort, skip, take, includes).ToListAsync();
            }
        }

        private IEnumerable<TEntity> Query(
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> sort = null,
            int? skip = null,
            int? take = null,
            params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
        {
            using (var context = GetDataContext())
            {
                return AggregateQuery(context, predicate, sort, skip, take, includes).ToList();
            }
        }

        /// <summary>
        ///     Creates an instance of the <see cref="DbContext" />
        /// </summary>
        /// <returns>An instance of the <see cref="DbContext" /></returns>
        /// <remarks>This should always be disposed of afterwards</remarks>
        protected virtual TContext GetDataContext()
        {
            return Activator.CreateInstance<TContext>();
        }
    }
}