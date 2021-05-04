using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using Microsoft.Extensions.DependencyInjection;
using static BlazorBase.CRUD.Models.IBaseModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.MessageHandling.Interfaces;

namespace BlazorBase.CRUD.Services
{
    public class BaseService
    {
        public DbContext DbContext { get; protected set; }
        public IServiceProvider ServiceProvider { get; }
        protected IMessageHandler MessageHandler { get; set; }

        public BaseService(DbContext context, IServiceProvider provider, IMessageHandler messageHandler)
        {
            DbContext = context;
            ServiceProvider = provider;
            MessageHandler = messageHandler;
        }

        public async void RefreshDbContext()
        {
            await DbContext.DisposeAsync();
            DbContext = ServiceProvider.GetService<DbContext>();
        }



        #region GetData

        public async virtual Task<T> GetAsync<T>(params object[] keyValues) where T : class
        {
            return await DbContext.Set<T>().FindAsync(keyValues);
        }

        public async virtual Task<object> GetAsync(Type type, params object[] keyValues)
        {
            return await DbContext.FindAsync(type, keyValues);
        }

        public async virtual Task<List<T>> GetDataAsync<T>() where T : class
        {
            return await DbContext.Set<T>().ToListAsync();
        }
        public async virtual Task<List<T>> GetDataAsync<T>(int index, int count) where T : class
        {
            return await DbContext.Set<T>().Skip(index).Take(count).ToListAsync();
        }

        public virtual Task<List<T>> GetDataAsync<T>(Func<T, bool> dataLoadCondition) where T : class
        {
            return Task.FromResult(DbContext.Set<T>().Where(dataLoadCondition).ToList());
        }
        public virtual Task<List<T>> GetDataAsync<T>(Func<T, bool> dataLoadCondition, int index, int count) where T : class
        {
            return Task.FromResult(DbContext.Set<T>().Where(dataLoadCondition).Skip(index).Take(count).ToList());
        }

        /// <summary>
        /// Super Slow -> use only if neccessary!
        /// </summary>
        public async virtual Task<List<object>> GetDataAsync(Type type)
        {
            return await DbContext.Set(type).ToListAsync();
        }

        /// <summary>
        /// Super Slow -> use only if neccessary!
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

        #region Count Data
        public async virtual Task<int> CountDataAsync<T>() where T : class
        {
            return await DbContext.Set<T>().CountAsync();
        }
        public virtual Task<int> CountDataAsync<T>(Func<T, bool> dataLoadCondition) where T : class
        {
            return Task.FromResult(DbContext.Set<T>().Where(dataLoadCondition).Count());
        }
        #endregion


        public async virtual Task ReloadAsync<T>(T entry) where T : class
        {
            await DbContext.Entry(entry).ReloadAsync();
        }

        public async virtual Task<bool> AddEntryAsync<T>(T entry) where T : class, IBaseModel
        {
            if (entry == null)
                return false;

            if (await DbContext.Set<T>().FindAsync(entry.GetPrimaryKeys()) != null)
                return false;

            DbContext.Set<T>().Add(entry);

            return true;
        }

        public virtual void UpdateEntry<T>(T entry) where T : class
        {
            DbContext.Set<T>().Update(entry);
        }

        public async virtual Task<bool> RemoveEntryAsync<T>(T entry) where T : class, IBaseModel
        {
            var entryToDelete = await DbContext.Set<T>().FindAsync(entry.GetPrimaryKeys());
            if (entryToDelete == null)
                return false;

            DbContext.Set<T>().Remove(entryToDelete);

            return true;
        }

        #region SaveChanges

      
        public async virtual Task<int> SaveChangesAsync()
        {
            var changedEntries = await HandleOnBeforeDbContextEvents();
            var result = await DbContext.SaveChangesAsync();
            await HandleOnAfterDbContextEvents(changedEntries);

            return result;
        }

        protected async Task<(List<EntityEntry> Added, List<EntityEntry> Modified, List<EntityEntry> Deleted)> HandleOnBeforeDbContextEvents()
        {
            DbContext.ChangeTracker.DetectChanges();

            var markedAsAdded = DbContext.ChangeTracker.Entries().Where(x => x.State == EntityState.Added).ToList();
            var markedAsModified = DbContext.ChangeTracker.Entries().Where(x => x.State == EntityState.Modified).ToList();
            var markedAsDeleted = DbContext.ChangeTracker.Entries().Where(x => x.State == EntityState.Deleted).ToList();

            var eventServices = GetEventServices();

            var addArgs = new OnBeforeDbContextAddEntryArgs(eventServices);
            foreach (var item in markedAsAdded)
                if (item.Entity is IBaseModel model)
                    await model.OnBeforeDbContextAddEntry(addArgs);

            var modifyArgs = new OnBeforeDbContextModifyEntryArgs(eventServices);
            foreach (var item in markedAsModified)
                if (item.Entity is IBaseModel model)
                    await model.OnBeforeDbContextModifyEntry(modifyArgs);

            var deleteArgs = new OnBeforeDbContextDeleteEntryArgs(eventServices);
            foreach (var item in markedAsDeleted)
                if (item.Entity is IBaseModel model)
                    await model.OnBeforeDbContextDeleteEntry(deleteArgs);

            return (markedAsAdded, markedAsModified, markedAsDeleted);
        }

        protected async Task HandleOnAfterDbContextEvents((List<EntityEntry> Added, List<EntityEntry> Modified, List<EntityEntry> Deleted) changedEntries)
        {
            var eventServices = GetEventServices();

            var addArgs = new OnAfterDbContextAddedEntryArgs(eventServices);
            foreach (var item in changedEntries.Added)
                if (item.Entity is IBaseModel model)
                    await model.OnAfterDbContextAddedEntry(addArgs);

            var modifyArgs = new OnAfterDbContextModifiedEntryArgs(eventServices);
            foreach (var item in changedEntries.Modified)
                if (item.Entity is IBaseModel model)
                    await model.OnAfterDbContextModifiedEntry(modifyArgs);

            var deleteArgs = new OnAfterDbContextDeletedEntryArgs(eventServices);
            foreach (var item in changedEntries.Deleted)
                if (item.Entity is IBaseModel model)
                    await model.OnAfterDbContextDeletedEntry(deleteArgs);
        }
        #endregion

        #region Other
        protected EventServices GetEventServices()
        {
            return new EventServices()
            {
                ServiceProvider = ServiceProvider,
                Localizer = null,
                BaseService = this,
                MessageHandler = MessageHandler
            };
        }
        #endregion
    }
}
