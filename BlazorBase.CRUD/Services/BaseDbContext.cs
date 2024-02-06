using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ViewModels;
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

namespace BlazorBase.CRUD.Services;

/// <summary>
/// Thread safe db context
/// </summary>
public class BaseDbContext(DbContext dbContext, IServiceProvider serviceProvider, IStringLocalizer<BaseDbContext> localizer) : IBaseDbContext
{
    #region Injects
    protected DbContext DbContext = dbContext;
    protected readonly IServiceProvider ServiceProvider = serviceProvider;
    #endregion

    #region Properties
    public ChangeTracker ChangeTracker { get { return DbContext.ChangeTracker; } }

    public IStringLocalizer Localizer { get; set; } = localizer;

    public SemaphoreSlim Semaphore { get; init; } = new(1, 1);
    #endregion

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
    public virtual IQueryable<object> Set(Type type)
    {
        return DbContext.Set(type);
    }

    public virtual IQueryable<T> Set<T>() where T : class
    {
        return DbContext.Set<T>();
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
    public virtual async Task<bool> ExistsTSAsync<T>(IBaseModel baseModel) where T: class, IBaseModel
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return DbContext.Find<T>(baseModel.GetPrimaryKeys()) != null;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<bool> ExistsAsyncTSAsync<T>(IBaseModel baseModel) where T : class, IBaseModel
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return await DbContext.FindAsync<T>(baseModel.GetPrimaryKeys()).ConfigureAwait(false) != null;
        }
        finally
        {
            Semaphore.Release();
        }
    }
    #endregion

    #region Find

    public virtual async Task<object?> FindTSAsync(Type entityType, params object?[]? keyValues)
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return DbContext.Find(entityType, keyValues);
        }
        finally
        {
            Semaphore.Release();
        }
    }


    public virtual async Task<object?> FindTSAsync(Type entityType, CancellationToken cancellationToken, params object?[]? keyValues)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return DbContext.Find(entityType, keyValues);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<T?> FindTSAsync<T>(params object?[]? keyValues) where T : class
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return DbContext.Find<T>(keyValues);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<T?> FindTSAsync<T>(CancellationToken cancellationToken, params object?[]? keyValues) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return DbContext.Find<T>(keyValues);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async ValueTask<object?> FindAsyncTSAsync(Type entityType, params object?[]? keyValues)
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return await DbContext.FindAsync(entityType, keyValues).ConfigureAwait(false);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async ValueTask<object?> FindAsyncTSAsync(Type entityType, CancellationToken cancellationToken, params object?[]? keyValues)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await DbContext.FindAsync(entityType, keyValues, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async ValueTask<T?> FindAsyncTSAsync<T>(params object?[]? keyValues) where T : class
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return await DbContext.FindAsync<T>(keyValues).ConfigureAwait(false);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async ValueTask<T?> FindAsyncTSAsync<T>(CancellationToken cancellationToken, params object?[]? keyValues) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await DbContext.FindAsync<T>(keyValues).ConfigureAwait(false);
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

    public virtual Task<T?> FindWithAllNavigationPropertiesTSAsync<T>(params object?[]? keyValues) where T : class
    {
        return FindWithAllNavigationPropertiesTSAsync<T>(new List<string>(), keyValues);
    }

    public async virtual Task<T?> FindWithAllNavigationPropertiesTSAsync<T>(IEnumerable<string> skipNavigationList, params object?[]? keyValues) where T : class
    {
        var entry = await FindTSAsync<T>(keyValues);
        if (entry == null)
            return null;

        await LoadAllNavigationPropertiesTSAsync(entry, skipNavigationList);

        return entry;
    }

    #endregion

    #region Where

    public virtual async Task<List<T>> WhereTSAsync<T>(Expression<Func<T, bool>> dataLoadCondition, bool asNoTracking = false, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var query = DbContext.Set<T>().Where(dataLoadCondition);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListTSAsync(this, cancellationToken);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<List<T>> WhereAsyncTSAsync<T>(Expression<Func<T, bool>> dataLoadCondition, bool asNoTracking = false, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var query = DbContext.Set<T>().Where(dataLoadCondition);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsyncTSAsync(this, cancellationToken);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<List<M>> WhereTSAsync<T, M>(Expression<Func<T, bool>> dataLoadCondition, Expression<Func<T, M>> dataSelectCondition, bool asNoTracking = false, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var query = DbContext.Set<T>().Where(dataLoadCondition);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.Select(dataSelectCondition).ToListTSAsync(this, cancellationToken);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<List<M>> WhereAsyncTSAsync<T, M>(Expression<Func<T, bool>> dataLoadCondition, Expression<Func<T, M>> dataSelectCondition, bool asNoTracking = false, CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var query = DbContext.Set<T>().Where(dataLoadCondition);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.Select(dataSelectCondition).ToListAsyncTSAsync(this, cancellationToken);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    #endregion

    #region Get New Primary Key

    public virtual async Task<Guid> GetNewPrimaryKeyTSAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Guid guid;
            do
            {
                guid = Guid.NewGuid();
            } while (DbContext.Find<T>(guid) != null);

            return guid;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async virtual Task<Guid> GetNewPrimaryKeyAsyncTSAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Guid guid;
            do
            {
                guid = Guid.NewGuid();
            } while (await DbContext.FindAsync<T>(guid).ConfigureAwait(false) != null);

            return guid;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task<Guid> GetNewPrimaryKeyTSAsync(Type type, CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Guid guid;
            do
            {
                guid = Guid.NewGuid();
            } while (DbContext.Find(type, guid) != null);

            return guid;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async virtual Task<Guid> GetNewPrimaryKeyAsyncTSAsync(Type type, CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Guid guid;
            do
            {
                guid = Guid.NewGuid();
            } while (await DbContext.FindAsync(type, guid).ConfigureAwait(false) != null);

            return guid;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    #endregion

    #region Load Data

    public async virtual Task LoadAllNavigationPropertiesTSAsync<T>(T entry, IEnumerable<string>? skipNavigationList = null, CancellationToken cancellationToken = default) where T : class
    {
        var navigationProperties = DbContext.Model.FindEntityType(typeof(T))?.GetNavigations();
        if (navigationProperties == null)
            return;

        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            foreach (var navigationProperty in navigationProperties)
            {
                if (skipNavigationList != null && skipNavigationList.Contains(navigationProperty.Name))
                    continue;

                if (navigationProperty.IsCollection)
                    DbContext.Entry(entry).Collection(navigationProperty.Name).Load();
                else
                    DbContext.Entry(entry).Reference(navigationProperty.Name).Load();
            }
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async virtual Task<T?> ReloadAllNavigationPropertiesTSAsync<T>(T entry, IEnumerable<string> skipNavigationList, CancellationToken cancellationToken = default) where T : class
    {
        var navigationProperties = DbContext.Model.FindEntityType(typeof(T))?.GetNavigations();
        if (navigationProperties == null)
            return entry;

        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            foreach (var navigationProperty in navigationProperties)
            {
                if (skipNavigationList.Contains(navigationProperty.Name))
                    continue;

                if (navigationProperty.IsCollection)
                    DbContext.Entry(entry).Collection(navigationProperty.Name).Load();
                else
                    DbContext.Entry(entry).Reference(navigationProperty.Name).Load();
            }

            return entry;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    #region Load Reference

    public virtual async Task LoadReferenceTSAsync<TEntity, TProperty>(EntityEntry<TEntity> entityEntry, Expression<Func<TEntity, TProperty?>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class
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

            entityEntry.Reference(propertyExpression).Load();

            if (entityIsInDeletionMode)
                entityEntry.State = EntityState.Deleted;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task LoadReferenceAsyncTSAsync<TEntity, TProperty>(EntityEntry<TEntity> entityEntry, Expression<Func<TEntity, TProperty?>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class
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

            await entityEntry.Reference(propertyExpression).LoadAsync(cancellationToken);

            if (entityIsInDeletionMode)
                entityEntry.State = EntityState.Deleted;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task LoadReferenceTSAsync<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty?>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class
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

            entityEntry.Reference(propertyExpression).Load();

            if (entityIsInDeletionMode)
                entityEntry.State = EntityState.Deleted;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task LoadReferenceAsyncTSAsync<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty?>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class
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

            await entityEntry.Reference(propertyExpression).LoadAsync(cancellationToken);

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

    public virtual async Task LoadCollectionTSAsync<TEntity, TProperty>(EntityEntry<TEntity> entityEntry, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            entityEntry.Collection(propertyExpression).Load();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task LoadCollectionAsyncTSAsync<TEntity, TProperty>(EntityEntry<TEntity> entityEntry, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await entityEntry.Collection(propertyExpression).LoadAsync(cancellationToken);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task LoadCollectionTSAsync<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            DbContext.Entry(entity).Collection(propertyExpression).Load();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual async Task LoadCollectionAsyncTSAsync<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await DbContext.Entry(entity).Collection(propertyExpression).LoadAsync(cancellationToken);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    #endregion

    #endregion

    #region Has Unsaved Changes
    public async Task<bool> HasUnsavedChangesTSAsync(CancellationToken cancellationToken = default)
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
    /// Uses the sync SaveChanges method of the DbContext for that
    /// Triggers all on before and on after DbContext events of the changed IBaseModels
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">
    ///     Indicates whether <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" /> is called after
    ///     the changes have been sent successfully to the database.
    /// </param>
    /// <returns></returns>
    public async virtual Task<int> SaveChangesTSAsync(bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        var changedEntries = await HandleOnBeforeDbContextEvents().ConfigureAwait(false);
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        int result;
        try
        {
            result = DbContext.SaveChanges(acceptAllChangesOnSuccess);
        }
        finally
        {
            Semaphore.Release();
        }

        await HandleOnAfterDbContextEvents(changedEntries).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// Uses the async SaveChangesAsync method of the DbContext for that
    /// Triggers all on before and on after DbContext events of the changed IBaseModels
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">
    ///     Indicates whether <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" /> is called after
    ///     the changes have been sent successfully to the database.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns></returns>
    public async virtual Task<int> SaveChangesAsyncTSAsync(bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        var changedEntries = await HandleOnBeforeDbContextEvents().ConfigureAwait(false);
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        int result;
        try
        {
            result = await DbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            Semaphore.Release();
        }

        await HandleOnAfterDbContextEvents(changedEntries).ConfigureAwait(false);
        return result;
    }

    protected async Task<(List<EntityEntry> Added, List<EntityEntry> Modified, List<EntityEntry> Deleted)> HandleOnBeforeDbContextEvents()
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

    protected async Task HandleOnAfterDbContextEvents((List<EntityEntry> Added, List<EntityEntry> Modified, List<EntityEntry> Deleted) changedEntries)
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
