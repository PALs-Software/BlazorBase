using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Builder;
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
public interface IBaseDbContext
{
    #region Properties
    ChangeTracker ChangeTracker { get; }
    IStringLocalizer Localizer { get; set; }
    SemaphoreSlim Semaphore { get; init; }
    #endregion

    #region Refresh
    Task RefreshDbContextAsync(CancellationToken cancellationToken = default);
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

    #region Exists
    Task<bool> ExistsTSAsync<T>(IBaseModel baseModel) where T : class, IBaseModel;

    Task<bool> ExistsAsyncTSAsync<T>(IBaseModel baseModel) where T : class, IBaseModel;
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

    IQueryable<T> FindAll<T>(IEnumerable<T> args) where T : class;

    Task<T?> FindWithAllNavigationPropertiesTSAsync<T>(params object?[]? keyValues) where T : class;

    Task<T?> FindWithAllNavigationPropertiesTSAsync<T>(IEnumerable<string> skipNavigationList, params object?[]? keyValues) where T : class;

    #endregion

    #region Where

    Task<List<T>> WhereTSAsync<T>(Expression<Func<T, bool>> dataLoadCondition, bool asNoTracking = false, CancellationToken cancellationToken = default) where T : class;

    Task<List<T>> WhereAsyncTSAsync<T>(Expression<Func<T, bool>> dataLoadCondition, bool asNoTracking = false, CancellationToken cancellationToken = default) where T : class;

    Task<List<M>> WhereTSAsync<T, M>(Expression<Func<T, bool>> dataLoadCondition, Expression<Func<T, M>> dataSelectCondition, bool asNoTracking = false, CancellationToken cancellationToken = default) where T : class;

    Task<List<M>> WhereAsyncTSAsync<T, M>(Expression<Func<T, bool>> dataLoadCondition, Expression<Func<T, M>> dataSelectCondition, bool asNoTracking = false, CancellationToken cancellationToken = default) where T : class;

    #endregion

    #region Get New Primary Key
    Task<Guid> GetNewPrimaryKeyTSAsync<T>(CancellationToken cancellationToken = default) where T : class;

    Task<Guid> GetNewPrimaryKeyAsyncTSAsync<T>(CancellationToken cancellationToken = default) where T : class;

    Task<Guid> GetNewPrimaryKeyTSAsync(Type type, CancellationToken cancellationToken = default);

    Task<Guid> GetNewPrimaryKeyAsyncTSAsync(Type type, CancellationToken cancellationToken = default);

    #endregion

    #region Load Data
    Task LoadAllNavigationPropertiesTSAsync<T>(T entry, IEnumerable<string>? skipNavigationList = null, CancellationToken cancellationToken = default) where T : class;

    Task<T?> ReloadAllNavigationPropertiesTSAsync<T>(T entry, IEnumerable<string> skipNavigationList, CancellationToken cancellationToken = default) where T : class;

    #region Load Reference

    Task LoadReferenceTSAsync<TEntity, TProperty>(EntityEntry<TEntity> entityEntry, Expression<Func<TEntity, TProperty?>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class;

    Task LoadReferenceAsyncTSAsync<TEntity, TProperty>(EntityEntry<TEntity> entityEntry, Expression<Func<TEntity, TProperty?>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class;

    Task LoadReferenceTSAsync<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty?>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class;

    Task LoadReferenceAsyncTSAsync<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty?>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class;

    #endregion

    #region Load Collection

    Task LoadCollectionTSAsync<TEntity, TProperty>(EntityEntry<TEntity> entityEntry, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class;

    Task LoadCollectionAsyncTSAsync<TEntity, TProperty>(EntityEntry<TEntity> entityEntry, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class;

    Task LoadCollectionTSAsync<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class;

    Task LoadCollectionAsyncTSAsync<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class;

    #endregion


    #endregion

    #region Has Unsaved Changes
    Task<bool> HasUnsavedChangesTSAsync(CancellationToken cancellationToken = default);

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

    #region Database Migration
    public static void MigrateDatabase<TDbContext>(IApplicationBuilder app) where TDbContext : DbContext
    {
        using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        scope.ServiceProvider.GetRequiredService<TDbContext>().Database.Migrate();
    }
    #endregion
}
