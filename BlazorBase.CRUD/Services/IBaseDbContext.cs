using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Services;

public interface IBaseDbContext
{
    #region Properties
    ChangeTracker ChangeTracker { get; }
    IStringLocalizer Localizer { get; set; }
    SemaphoreSlim Semaphore { get; init; }
    #endregion

    #region Set
    /// <summary>
    /// Super Slow -> use only if neccessary!!!
    /// </summary>
    IQueryable<object> Set(Type type);

    IQueryable<T> Set<T>() where T : class;

    #endregion

    #region Add
    ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default);

    ValueTask<EntityEntry<T>> AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

    Task AddRangeAsync(params object[] entities);

    Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default);
    
    #endregion

    #region Update
    Task<EntityEntry> UpdateAsync(object entity, CancellationToken cancellationToken = default);

    Task<EntityEntry<T>> UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

    Task UpdateRangeAsync(params object[] entities);

    Task UpdateRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default);
    
    #endregion

    #region Attach
    Task<EntityEntry> AttachAsync(object entity, CancellationToken cancellationToken = default);

    Task<EntityEntry<T>> AttachAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

    Task AttachRangeAsync(params object[] entities);
    Task AttachRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default);
    
    #endregion

    #region Remove
    Task<EntityEntry> RemoveAsync(object entity, CancellationToken cancellationToken = default);

    Task<EntityEntry<T>> RemoveAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

    Task RemoveRangeAsync(params object[] entities);

    Task RemoveRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default);

    #endregion

    #region Entry
    Task<EntityEntry> EntryAsync(object entity, CancellationToken cancellationToken = default);

    Task<EntityEntry<T>> EntryAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

    #endregion

    #region Find

    Task<object?> FindTSAsync(Type entityType, params object?[]? keyValues);

    Task<object?> FindTSAsync(Type entityType, CancellationToken cancellationToken, params object?[]? keyValues);

    Task<T?> FindTSAsync<T>(params object?[]? keyValues) where T : class;

    Task<T?> FindTSAsync<T>(CancellationToken cancellationToken, params object?[]? keyValues) where T : class;

    ValueTask<object?> FindAsyncTSAsync(Type entityType, params object?[]? keyValues);

    ValueTask<object?> FindAsyncTSAsync(Type entityType, CancellationToken cancellationToken, params object?[]? keyValues);

    ValueTask<T?> FindAsyncTSAsync<T>(params object?[]? keyValues) where T : class;

    ValueTask<T?> FindAsyncTSAsync<T>(CancellationToken cancellationToken, params object?[]? keyValues) where T : class;

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
    Task<int> SaveChangesTSAsync(bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

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
    Task<int> SaveChangesAsyncTSAsync(bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    #endregion
}
