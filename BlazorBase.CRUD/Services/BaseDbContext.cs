using BlazorBase.Abstractions.CRUD.Arguments;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.Abstractions.CRUD.Structures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using BlazorBase.Abstractions.CRUD.Interfaces;

namespace BlazorBase.CRUD.Services;

/// <summary>
/// Thread safe db context
/// </summary>
public class BaseDbContext : IBaseDbContext
{
    #region Injects
    protected DbContext DbContext;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly IBlazorBaseCRUDOptions Options;
    #endregion

    #region Properties
    public ChangeTracker ChangeTracker { get { return DbContext.ChangeTracker; } }

    public IStringLocalizer Localizer { get; set; }

    /// <summary>
    /// Currently there is a performance problem in the .net SQLClient when data records are loaded from the database that contain large amounts of data of type string.
    /// This performance problem only occurs with the async methods.
    /// For this reason, it may be useful to use the sync methods of the db context.
    /// </summary>
    public bool UseAsyncDbContextMethods { get; set; }
    #endregion

    #region Members
    protected SemaphoreSlim Semaphore = new(1, 1);
    #endregion

    public BaseDbContext(DbContext dbContext, IServiceProvider serviceProvider, IStringLocalizer<BaseDbContext> localizer, IBlazorBaseCRUDOptions options)
    {
        DbContext = dbContext;
        ServiceProvider = serviceProvider;
        Localizer = localizer;
        Options = options;

        UseAsyncDbContextMethods = Options.UseAsyncDbContextMethodsPerDefaultInBaseDbContext;
    }

    #region Refresh
    public async Task RefreshDbContextAsync(CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await DbContext.DisposeAsync();
            DbContext = ServiceProvider.GetRequiredService<DbContext>();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    #endregion

    #region Set

    /// <summary>
    /// Super Slow -> use only if neccessary!!!
    /// </summary>
    public virtual async Task<TResult> SetAsync<TResult>(Type type, Func<IQueryable<object>, Task<TResult>> queryFunc, CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return await queryFunc.Invoke(DbContext.Set(type)).ConfigureAwait(false);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    /// <summary>
    /// Super Slow -> use only if neccessary!!!
    /// </summary>
    public virtual async Task<TResult> SetAsync<TResult>(Type type, Func<IQueryable<object>, TResult> queryFunc, CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return queryFunc.Invoke(DbContext.Set(type));
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<TResult> SetAsync<TEntity, TResult>(Func<IQueryable<TEntity>, Task<TResult>> queryFunc, CancellationToken cancellationToken = default) where TEntity : class
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return await queryFunc.Invoke(DbContext.Set<TEntity>()).ConfigureAwait(false);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<TResult> SetAsync<TEntity, TResult>(Func<IQueryable<TEntity>, TResult> queryFunc, CancellationToken cancellationToken = default) where TEntity : class
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return queryFunc.Invoke(DbContext.Set<TEntity>());
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task SetAsync<TEntity>(Action<IQueryable<TEntity>> queryAction, CancellationToken cancellationToken = default) where TEntity : class
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            queryAction.Invoke(DbContext.Set<TEntity>());
        }
        finally
        {
            Semaphore.Release();
        }
    }

    #endregion

    #region Add

    public virtual async ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return DbContext.Add(entity);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async ValueTask<EntityEntry<T>> AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return DbContext.Add(entity);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task AddRangeAsync(params object[] entities)
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            DbContext.AddRange(entities);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            DbContext.AddRange(entities);
        }
        finally
        {
            Semaphore.Release();
        }
    }
    #endregion

    #region Update
    public virtual async Task<EntityEntry> UpdateAsync(object entity, CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return DbContext.Update(entity);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<EntityEntry<T>> UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return DbContext.Update(entity);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task UpdateRangeAsync(params object[] entities)
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            DbContext.UpdateRange(entities);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            DbContext.UpdateRange(entities);
        }
        finally
        {
            Semaphore.Release();
        }
    }
    #endregion

    #region Attach
    public virtual async Task<EntityEntry> AttachAsync(object entity, CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return DbContext.Attach(entity);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<EntityEntry<T>> AttachAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return DbContext.Attach(entity);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task AttachRangeAsync(params object[] entities)
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            DbContext.AttachRange(entities);
        }
        finally
        {
            Semaphore.Release();
        }
    }
    public virtual async Task AttachRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            DbContext.AttachRange(entities);
        }
        finally
        {
            Semaphore.Release();
        }
    }
    #endregion

    #region Remove
    public virtual async Task<EntityEntry> RemoveAsync(object entity, CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return DbContext.Remove(entity);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<EntityEntry<T>> RemoveAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return DbContext.Remove(entity);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task RemoveRangeAsync(params object[] entities)
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            DbContext.RemoveRange(entities);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task RemoveRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            DbContext.RemoveRange(entities);
        }
        finally
        {
            Semaphore.Release();
        }
    }
    #endregion

    #region Entry

    public virtual async Task<EntityEntry> EntryAsync(object entity, CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return DbContext.Entry(entity);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<EntityEntry<T>> EntryAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return DbContext.Entry(entity);
        }
        finally
        {
            Semaphore.Release();
        }
    }
    #endregion

    #region Exists
    public virtual async Task<bool> ExistsAsync<T>(IBaseModel baseModel, CancellationToken cancellationToken = default, bool? useAsyncDbContextMethod = null) where T : class, IBaseModel
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value)
                return await DbContext.FindAsync<T>(baseModel.GetPrimaryKeys(), cancellationToken).ConfigureAwait(false) != null;
            else
                return DbContext.Find<T>(baseModel.GetPrimaryKeys()) != null;
        }
        finally
        {
            Semaphore.Release();
        }
    }
    #endregion

    #region Any
    public virtual async Task<bool> AnyAsync<T>(Expression<Func<T, bool>> anyCondition, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value)
                return await DbContext.Set<T>().AnyAsync(anyCondition, cancellationToken).ConfigureAwait(false);
            else
                return DbContext.Set<T>().Any(anyCondition);
        }
        finally
        {
            Semaphore.Release();
        }
    }
    #endregion

    #region Find

    public virtual Task<object?> FindAsync(Type entityType, params object?[]? keyValues)
    {
        return FindAsync(entityType, default, UseAsyncDbContextMethods, keyValues);
    }

    public virtual Task<object?> FindAsync(Type entityType, bool useAsyncDbContextMethod, params object?[]? keyValues)
    {
        return FindAsync(entityType, default, useAsyncDbContextMethod, keyValues);
    }

    public virtual Task<object?> FindAsync(Type entityType, CancellationToken cancellationToken, params object?[]? keyValues)
    {
        return FindAsync(entityType, cancellationToken, UseAsyncDbContextMethods, keyValues);
    }

    public virtual async Task<object?> FindAsync(Type entityType, CancellationToken cancellationToken, bool useAsyncDbContextMethod, params object?[]? keyValues)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (useAsyncDbContextMethod)
                return await DbContext.FindAsync(entityType, keyValues).ConfigureAwait(false);
            else
                return DbContext.Find(entityType, keyValues);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual Task<T?> FindAsync<T>(params object?[]? keyValues) where T : class
    {
        return FindAsync<T>(default, UseAsyncDbContextMethods, keyValues);
    }

    public virtual Task<T?> FindAsync<T>(CancellationToken cancellationToken, params object?[]? keyValues) where T : class
    {
        return FindAsync<T>(cancellationToken, UseAsyncDbContextMethods, keyValues);
    }

    public virtual Task<T?> FindAsync<T>(bool useAsyncDbContextMethod, params object?[]? keyValues) where T : class
    {
        return FindAsync<T>(default, useAsyncDbContextMethod, keyValues);
    }

    public virtual async Task<T?> FindAsync<T>(CancellationToken cancellationToken, bool useAsyncDbContextMethod, params object?[]? keyValues) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (useAsyncDbContextMethod)
                return await DbContext.FindAsync<T>(keyValues).ConfigureAwait(false);
            else
                return DbContext.Find<T>(keyValues);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public IQueryable<T> FindAll<T>(IEnumerable<T> args) where T : class
    {
        var dbParameter = Expression.Parameter(typeof(T), typeof(T).Name);
        var properties = DbContext.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties;

        if (properties == null)
            throw new ArgumentException($"{typeof(T).FullName} does not have a primary key specified.");
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        if (!args.Any())
            return DbContext.Set<T>();

        var aggregatedExpression = args.Select(entity =>
        {
            var entry = DbContext.Entry(entity);

            return properties.Select(p =>
            {
                var dbProp = dbParameter.Type.GetProperty(p.Name);
                ArgumentNullException.ThrowIfNull(dbProp);
                var left = Expression.Property(dbParameter, dbProp);

                var argValue = entry.Property(p.Name).CurrentValue;
                var right = Expression.Constant(argValue);

                return Expression.Equal(left, right);
            })
            .Aggregate((acc, next) => Expression.AndAlso(acc, next));
        })
        .Aggregate(Expression.OrElse);

        var expression = Expression.Lambda<Func<T, bool>>(aggregatedExpression, dbParameter);

        return DbContext.Set<T>().Where(expression);
    }

    public virtual Task<T?> FindWithAllNavigationPropertiesAsync<T>(params object?[]? keyValues) where T : class
    {
        return FindWithAllNavigationPropertiesAsync<T>(new List<string>(), UseAsyncDbContextMethods, default, keyValues);
    }

    public virtual Task<T?> FindWithAllNavigationPropertiesAsync<T>(CancellationToken cancellationToken, params object?[]? keyValues) where T : class
    {
        return FindWithAllNavigationPropertiesAsync<T>(new List<string>(), UseAsyncDbContextMethods, cancellationToken, keyValues);
    }

    public virtual Task<T?> FindWithAllNavigationPropertiesAsync<T>(bool useAsyncDbContextMethod, params object?[]? keyValues) where T : class
    {
        return FindWithAllNavigationPropertiesAsync<T>(new List<string>(), useAsyncDbContextMethod, keyValues);
    }

    public virtual Task<T?> FindWithAllNavigationPropertiesAsync<T>(IEnumerable<string> skipNavigationList, params object?[]? keyValues) where T : class
    {
        return FindWithAllNavigationPropertiesAsync<T>(skipNavigationList, UseAsyncDbContextMethods, default, keyValues);
    }

    public virtual Task<T?> FindWithAllNavigationPropertiesAsync<T>(IEnumerable<string> skipNavigationList, CancellationToken cancellationToken, params object?[]? keyValues) where T : class
    {
        return FindWithAllNavigationPropertiesAsync<T>(skipNavigationList, UseAsyncDbContextMethods, cancellationToken, keyValues);
    }

    public virtual Task<T?> FindWithAllNavigationPropertiesAsync<T>(IEnumerable<string> skipNavigationList, bool useAsyncDbContextMethod, params object?[]? keyValues) where T : class
    {
        return FindWithAllNavigationPropertiesAsync<T>(skipNavigationList, useAsyncDbContextMethod, default, keyValues);
    }

    public async virtual Task<T?> FindWithAllNavigationPropertiesAsync<T>(IEnumerable<string> skipNavigationList, bool useAsyncDbContextMethod, CancellationToken cancellationToken, params object?[]? keyValues) where T : class
    {
        var entry = await FindAsync<T>(keyValues).ConfigureAwait(false);
        if (entry == null)
            return null;

        await LoadAllNavigationPropertiesAsync(entry, skipNavigationList, useAsyncDbContextMethod, cancellationToken).ConfigureAwait(false);

        return entry;
    }

    #endregion

    #region FirstOrDefault

    public virtual async Task<T> FirstAsync<T>(bool asNoTracking = false, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            IQueryable<T> query = DbContext.Set<T>();

            if (asNoTracking)
                query = query.AsNoTracking();

            if (useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value)
                return await query.FirstAsync(cancellationToken).ConfigureAwait(false);
            else
                return query.First();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<T> FirstAsync<T>(Expression<Func<T, bool>> dataLoadCondition, bool asNoTracking = false, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            IQueryable<T> query = DbContext.Set<T>();

            if (asNoTracking)
                query = query.AsNoTracking();

            if (useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value)
                return await query.FirstAsync(dataLoadCondition, cancellationToken).ConfigureAwait(false);
            else
                return query.First(dataLoadCondition);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<T?> FirstOrDefaultAsync<T>(bool asNoTracking = false, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            IQueryable<T> query = DbContext.Set<T>();

            if (asNoTracking)
                query = query.AsNoTracking();

            if (useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value)
                return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            else
                return query.FirstOrDefault();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<T?> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> dataLoadCondition, bool asNoTracking = false, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            IQueryable<T> query = DbContext.Set<T>();

            if (asNoTracking)
                query = query.AsNoTracking();

            if (useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value)
                return await query.FirstOrDefaultAsync(dataLoadCondition, cancellationToken).ConfigureAwait(false);
            else
                return query.FirstOrDefault(dataLoadCondition);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    #endregion

    #region Where

    public virtual async Task<List<T>> WhereAsync<T>(Expression<Func<T, bool>> dataLoadCondition, bool asNoTracking = false, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var query = DbContext.Set<T>().Where(dataLoadCondition);

            if (asNoTracking)
                query = query.AsNoTracking();

            if (useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value)
                return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            else
                return query.ToList();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<List<M>> WhereAsync<T, M>(Expression<Func<T, bool>> dataLoadCondition, Expression<Func<T, M>> dataSelectCondition, bool asNoTracking = false, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var query = DbContext.Set<T>().Where(dataLoadCondition);

            if (asNoTracking)
                query = query.AsNoTracking();

            if (useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value)
                return await query.Select(dataSelectCondition).ToListAsync(cancellationToken).ConfigureAwait(false);
            else
                return query.Select(dataSelectCondition).ToList();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    #endregion

    #region Get New Primary Key

    public virtual async Task<Guid> GetNewPrimaryKeyAsync<T>(bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Guid guid;
            object? objectInstance;
            do
            {
                guid = Guid.NewGuid();

                if (useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value)
                    objectInstance = await DbContext.FindAsync<T>(guid).ConfigureAwait(false);
                else
                    objectInstance = DbContext.Find<T>(guid);

            } while (objectInstance != null);

            return guid;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<Guid> GetNewPrimaryKeyAsync(Type type, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Guid guid;
            object? objectInstance;
            do
            {
                guid = Guid.NewGuid();

                if (useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value)
                    objectInstance = await DbContext.FindAsync(type, guid).ConfigureAwait(false);
                else
                    objectInstance = DbContext.Find(type, guid);

            } while (objectInstance != null);

            return guid;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    #endregion

    #region Load Data

    public async virtual Task LoadAllNavigationPropertiesAsync<T>(T entry, IEnumerable<string>? skipNavigationList = null, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class
    {
        var navigationProperties = DbContext.Model.FindEntityType(typeof(T))?.GetNavigations();
        if (navigationProperties == null)
            return;

        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        var useAsyncMethods = useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value;
        try
        {
            foreach (var navigationProperty in navigationProperties)
            {
                if (skipNavigationList != null && skipNavigationList.Contains(navigationProperty.Name))
                    continue;

                Microsoft.EntityFrameworkCore.ChangeTracking.NavigationEntry navigationEntry;
                if (navigationProperty.IsCollection)
                    navigationEntry = DbContext.Entry(entry).Collection(navigationProperty.Name);
                else
                    navigationEntry = DbContext.Entry(entry).Reference(navigationProperty.Name);

                if (useAsyncMethods)
                    await navigationEntry.LoadAsync(cancellationToken).ConfigureAwait(false);
                else
                    navigationEntry.Load();
            }
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async virtual Task<T?> ReloadAllNavigationPropertiesAsync<T>(T entry, IEnumerable<string> skipNavigationList, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class
    {
        var navigationProperties = DbContext.Model.FindEntityType(typeof(T))?.GetNavigations();
        if (navigationProperties == null)
            return entry;

        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        var useAsyncMethods = useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value;
        try
        {
            foreach (var navigationProperty in navigationProperties)
            {
                if (skipNavigationList.Contains(navigationProperty.Name))
                    continue;

                Microsoft.EntityFrameworkCore.ChangeTracking.NavigationEntry navigationEntry;
                if (navigationProperty.IsCollection)
                    navigationEntry = DbContext.Entry(entry).Collection(navigationProperty.Name);
                else
                    navigationEntry = DbContext.Entry(entry).Reference(navigationProperty.Name);

                if (useAsyncMethods)
                    await navigationEntry.LoadAsync(cancellationToken).ConfigureAwait(false);
                else
                    navigationEntry.Load();
            }

            return entry;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    #region Load Reference

    public virtual async Task LoadReferenceAsync<TEntity, TProperty>(EntityEntry<TEntity> entityEntry, Expression<Func<TEntity, TProperty?>> propertyExpression, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var entityIsInDeletionMode = false;
            if (entityEntry.State == EntityState.Deleted) // Temporarily change deleted state, otherwise the reference cannot be loaded
            {
                entityEntry.State = EntityState.Modified;
                entityIsInDeletionMode = true;
            }

            if (useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value)
                await entityEntry.Reference(propertyExpression).LoadAsync(cancellationToken).ConfigureAwait(false);
            else
                entityEntry.Reference(propertyExpression).Load();

            if (entityIsInDeletionMode)
                entityEntry.State = EntityState.Deleted;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task LoadReferenceAsync<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty?>> propertyExpression, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var entityIsInDeletionMode = false;
            var entityEntry = DbContext.Entry(entity);
            if (entityEntry.State == EntityState.Deleted) // Temporarily change deleted state, otherwise the reference cannot be loaded
            {
                entityEntry.State = EntityState.Modified;
                entityIsInDeletionMode = true;
            }

            if (useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value)
                await entityEntry.Reference(propertyExpression).LoadAsync(cancellationToken).ConfigureAwait(false);
            else
                entityEntry.Reference(propertyExpression).Load();

            if (entityIsInDeletionMode)
                entityEntry.State = EntityState.Deleted;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    #endregion

    #region Load Collection

    public virtual async Task LoadCollectionAsync<TEntity, TProperty>(EntityEntry<TEntity> entityEntry, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value)
                await entityEntry.Collection(propertyExpression).LoadAsync(cancellationToken);
            else
                entityEntry.Collection(propertyExpression).Load();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task LoadCollectionAsync<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value)
                await DbContext.Entry(entity).Collection(propertyExpression).LoadAsync(cancellationToken);
            else
                DbContext.Entry(entity).Collection(propertyExpression).Load();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    #endregion

    #endregion

    #region Has Unsaved Changes
    public async Task<bool> HasUnsavedChangesAsync(CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return DbContext.ChangeTracker.HasChanges();
        }
        catch (Exception) //Prevent Primary Key Null Error Issue
        {
            return true;
        }
        finally
        {
            Semaphore.Release();
        }
    }
    #endregion

    #region SaveChanges

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// Triggers all on before and on after DbContext events of the changed IBaseModels
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">
    ///     Indicates whether <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" /> is called after
    ///     the changes have been sent successfully to the database.
    /// </param>
    /// <returns></returns>
    public async virtual Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess = true, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default)
    {
        var changedEntries = await HandleOnBeforeDbContextEventsAsync().ConfigureAwait(false);
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        int result;
        try
        {
            if (useAsyncDbContextMethod == null && UseAsyncDbContextMethods || useAsyncDbContextMethod != null && useAsyncDbContextMethod.Value)
                result = await DbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
            else
                result = DbContext.SaveChanges(acceptAllChangesOnSuccess);
        }
        finally
        {
            Semaphore.Release();
        }

        await HandleOnAfterDbContextEventsAsync(changedEntries).ConfigureAwait(false);
        return result;
    }

    protected async Task<(List<EntityEntry> Added, List<EntityEntry> Modified, List<EntityEntry> Deleted)> HandleOnBeforeDbContextEventsAsync()
    {
        var entries = DbContext.ChangeTracker.Entries(); // Already runs internally DetectChanges()

        var markedAsAdded = entries.Where(x => x.State == EntityState.Added).ToList();
        var markedAsModified = entries.Where(x => x.State == EntityState.Modified).ToList();
        var markedAsDeleted = entries.Where(x => x.State == EntityState.Deleted).ToList();

        var eventServices = new EventServices(ServiceProvider, this, Localizer);

        var addArgs = new OnBeforeDbContextAddEntryArgs(eventServices, []);
        await Parallel.ForEachAsync(markedAsAdded, async (item, cancellationToken) =>
        {
            if (item.Entity is IBaseModel model)
                await model.OnBeforeDbContextAddEntry(addArgs).ConfigureAwait(false);
        }).ConfigureAwait(false);
        await CascadeOnBeforeDbContextAddEntryEventAsync(addArgs, markedAsAdded, eventServices).ConfigureAwait(false);

        var modifyArgs = new OnBeforeDbContextModifyEntryArgs(eventServices, []);
        await Parallel.ForEachAsync(markedAsModified, async (item, cancellationToken) =>
        {
            if (item.Entity is IBaseModel model)
                await model.OnBeforeDbContextModifyEntry(modifyArgs).ConfigureAwait(false);
        }).ConfigureAwait(false);
        await CascadeOnBeforeDbContextModifyEntryEventAsync(modifyArgs, markedAsModified, eventServices).ConfigureAwait(false);

        var deleteArgs = new OnBeforeDbContextDeleteEntryArgs(eventServices, []);
        await Parallel.ForEachAsync(markedAsDeleted, async (item, cancellationToken) =>
        {
            if (item.Entity is IBaseModel model)
                await model.OnBeforeDbContextDeleteEntry(deleteArgs).ConfigureAwait(false);
        }).ConfigureAwait(false);
        await CascadeOnBeforeDbContextDeleteEntryEventAsync(deleteArgs, markedAsDeleted, eventServices).ConfigureAwait(false);

        return (markedAsAdded, markedAsModified, markedAsDeleted);
    }

    protected virtual async Task CascadeOnBeforeDbContextAddEntryEventAsync(OnBeforeDbContextAddEntryArgs addEntryArgs, List<EntityEntry> markedAsAdded, EventServices eventServices)
    {
        while (addEntryArgs.AdditionalEntriesAdded.Count > 0)
        {
            var addEntryArgs2 = new OnBeforeDbContextAddEntryArgs(eventServices, []);
            await Parallel.ForEachAsync(addEntryArgs.AdditionalEntriesAdded, async (item, cancellationToken) =>
            {
                var entityEntry = DbContext.Entry(item);
                if (entityEntry.State == EntityState.Added)
                {
                    markedAsAdded.Add(entityEntry);

                    if (entityEntry.Entity is IBaseModel model)
                        await model.OnBeforeDbContextAddEntry(addEntryArgs2).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);

            addEntryArgs = addEntryArgs2;
        }
    }

    protected virtual async Task CascadeOnBeforeDbContextModifyEntryEventAsync(OnBeforeDbContextModifyEntryArgs modifyEntryArgs, List<EntityEntry> markedAsModified, EventServices eventServices)
    {
        while (modifyEntryArgs.AdditionalEntriesModified.Count > 0)
        {
            var modifyEntryArgs2 = new OnBeforeDbContextModifyEntryArgs(eventServices, []);
            await Parallel.ForEachAsync(modifyEntryArgs.AdditionalEntriesModified, async (item, cancellationToken) =>
            {
                var entityEntry = DbContext.Entry(item);
                if (entityEntry.State == EntityState.Modified)
                {
                    markedAsModified.Add(entityEntry);

                    if (entityEntry.Entity is IBaseModel model)
                        await model.OnBeforeDbContextModifyEntry(modifyEntryArgs2).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);

            modifyEntryArgs = modifyEntryArgs2;
        }
    }

    protected virtual async Task CascadeOnBeforeDbContextDeleteEntryEventAsync(OnBeforeDbContextDeleteEntryArgs deleteEntryArgs, List<EntityEntry> markedAsDeleted, EventServices eventServices)
    {
        while (deleteEntryArgs.AdditionalEntriesDeleted.Count > 0)
        {
            var deleteEntryArgs2 = new OnBeforeDbContextDeleteEntryArgs(eventServices, []);
            await Parallel.ForEachAsync(deleteEntryArgs.AdditionalEntriesDeleted, async (item, cancellationToken) =>
            {
                var entityEntry = DbContext.Entry(item);
                if (entityEntry.State == EntityState.Deleted)
                {
                    markedAsDeleted.Add(entityEntry);

                    if (entityEntry.Entity is IBaseModel model)
                        await model.OnBeforeDbContextDeleteEntry(deleteEntryArgs2).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);

            deleteEntryArgs = deleteEntryArgs2;
        }
    }

    protected async Task HandleOnAfterDbContextEventsAsync((List<EntityEntry> Added, List<EntityEntry> Modified, List<EntityEntry> Deleted) changedEntries)
    {
        var eventServices = new EventServices(ServiceProvider, this, Localizer);

        var addArgs = new OnAfterDbContextAddedEntryArgs(eventServices);
        await Parallel.ForEachAsync(changedEntries.Added, async (item, cancellationToken) =>
        {
            if (item.Entity is IBaseModel model)
                await model.OnAfterDbContextAddedEntry(addArgs).ConfigureAwait(false);
        }).ConfigureAwait(false);

        var modifyArgs = new OnAfterDbContextModifiedEntryArgs(eventServices);
        await Parallel.ForEachAsync(changedEntries.Modified, async (item, cancellationToken) =>
        {
            if (item.Entity is IBaseModel model)
                await model.OnAfterDbContextModifiedEntry(modifyArgs).ConfigureAwait(false);
        }).ConfigureAwait(false);

        var deleteArgs = new OnAfterDbContextDeletedEntryArgs(eventServices);
        await Parallel.ForEachAsync(changedEntries.Deleted, async (item, cancellationToken) =>
        {
            if (item.Entity is IBaseModel model)
                await model.OnAfterDbContextDeletedEntry(deleteArgs).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }
    #endregion

}
