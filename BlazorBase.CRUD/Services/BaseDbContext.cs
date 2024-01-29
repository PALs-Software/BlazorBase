using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Services;

/// <summary>
/// Thread safe db context
/// </summary>
public class BaseDbContext(DbContext dbContext, IServiceProvider serviceProvider, IStringLocalizer<BaseDbContext> localizer) : IBaseDbContext
{
    #region Injects
    protected readonly DbContext DbContext = dbContext;
    protected readonly IServiceProvider ServiceProvider = serviceProvider;
    #endregion

    #region Properties
    public ChangeTracker ChangeTracker { get { return DbContext.ChangeTracker; } }

    public IStringLocalizer Localizer { get; set; } = localizer;

    public SemaphoreSlim Semaphore { get; init; } = new(1, 1);
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
