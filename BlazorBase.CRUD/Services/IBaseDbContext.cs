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

    /// <summary>
    /// Currently there is a performance problem in the .net SQLClient when data records are loaded from the database that contain large amounts of data of type string.
    /// This performance problem only occurs with the async methods.
    /// For this reason, it may be useful to use the sync methods of the db context.
    /// </summary>
    bool UseAsyncDbContextMethods { get; set; }
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
    Task<bool> ExistsAsync<T>(IBaseModel baseModel, CancellationToken cancellationToken = default, bool? useAsyncDbContextMethod = null) where T : class, IBaseModel;
    #endregion

    #region Any
    Task<bool> AnyAsync<T>(Expression<Func<T, bool>> anyCondition, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class;
    #endregion

    #region Find
    Task<object?> FindAsync(Type entityType, params object?[]? keyValues);

    Task<object?> FindAsync(Type entityType, bool useAsyncDbContextMethod, params object?[]? keyValues);

    Task<object?> FindAsync(Type entityType, CancellationToken cancellationToken, params object?[]? keyValues);

    Task<object?> FindAsync(Type entityType, CancellationToken cancellationToken, bool useAsyncDbContextMethod, params object?[]? keyValues);

    Task<T?> FindAsync<T>(params object?[]? keyValues) where T : class;

    Task<T?> FindAsync<T>(CancellationToken cancellationToken, params object?[]? keyValues) where T : class;

    Task<T?> FindAsync<T>(bool useAsyncDbContextMethod, params object?[]? keyValues) where T : class;

    Task<T?> FindAsync<T>(CancellationToken cancellationToken, bool useAsyncDbContextMethod, params object?[]? keyValues) where T : class;

    IQueryable<T> FindAll<T>(IEnumerable<T> args) where T : class;

    Task<T?> FindWithAllNavigationPropertiesAsync<T>(params object?[]? keyValues) where T : class;
    Task<T?> FindWithAllNavigationPropertiesAsync<T>(CancellationToken cancellationToken, params object?[]? keyValues) where T : class;
    Task<T?> FindWithAllNavigationPropertiesAsync<T>(bool useAsyncDbContextMethod, params object?[]? keyValues) where T : class;
    Task<T?> FindWithAllNavigationPropertiesAsync<T>(IEnumerable<string> skipNavigationList, params object?[]? keyValues) where T : class;
    Task<T?> FindWithAllNavigationPropertiesAsync<T>(IEnumerable<string> skipNavigationList, CancellationToken cancellationToken, params object?[]? keyValues) where T : class;
    Task<T?> FindWithAllNavigationPropertiesAsync<T>(IEnumerable<string> skipNavigationList, bool useAsyncDbContextMethod, params object?[]? keyValues) where T : class;
    Task<T?> FindWithAllNavigationPropertiesAsync<T>(IEnumerable<string> skipNavigationList, bool useAsyncDbContextMethod, CancellationToken cancellationToken, params object?[]? keyValues) where T : class;
    #endregion

    #region Where

    Task<List<T>> WhereAsync<T>(Expression<Func<T, bool>> dataLoadCondition, bool asNoTracking = false, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class;

    Task<List<M>> WhereAsync<T, M>(Expression<Func<T, bool>> dataLoadCondition, Expression<Func<T, M>> dataSelectCondition, bool asNoTracking = false, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class;

    #endregion

    #region Get New Primary Key
    Task<Guid> GetNewPrimaryKeyAsync<T>(bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class;

    Task<Guid> GetNewPrimaryKeyAsync(Type type, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default);

    #endregion

    #region Load Data
    Task LoadAllNavigationPropertiesAsync<T>(T entry, IEnumerable<string>? skipNavigationList = null, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class;

    Task<T?> ReloadAllNavigationPropertiesAsync<T>(T entry, IEnumerable<string> skipNavigationList, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where T : class;

    #region Load Reference

    Task LoadReferenceAsync<TEntity, TProperty>(EntityEntry<TEntity> entityEntry, Expression<Func<TEntity, TProperty?>> propertyExpression, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class;

    Task LoadReferenceAsync<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty?>> propertyExpression, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class;

    #endregion

    #region Load Collection

    Task LoadCollectionAsync<TEntity, TProperty>(EntityEntry<TEntity> entityEntry, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class;

    Task LoadCollectionAsync<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default) where TProperty : class where TEntity : class;

    #endregion

    #endregion

    #region Has Unsaved Changes
    Task<bool> HasUnsavedChangesAsync(CancellationToken cancellationToken = default);

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
    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess = true, bool? useAsyncDbContextMethod = null, CancellationToken cancellationToken = default);

    #endregion

    #region Database Migration
    public static void MigrateDatabase<TDbContext>(IApplicationBuilder app) where TDbContext : DbContext
    {
        using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        scope.ServiceProvider.GetRequiredService<TDbContext>().Database.Migrate();
    }
    #endregion
}
