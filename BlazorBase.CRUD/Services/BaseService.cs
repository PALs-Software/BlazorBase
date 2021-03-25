using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;

namespace BlazorBase.CRUD.Services
{
    public class BaseService
    {
        public DbContext DbContext { get; }

        public BaseService(DbContext context)
        {
            DbContext = context;
        }

        public async virtual Task<T> GetAsync<T>(params object[] keyValues) where T : class
        {
            return await DbContext.Set<T>().FindAsync(keyValues);
        }

        public async virtual Task<List<T>> GetDataAsync<T>() where T : class
        {
            return await DbContext.Set<T>().ToListAsync();
        }

        public virtual Task<List<T>> GetDataAsync<T>(Func<T, bool> dataLoadCondition) where T : class
        {
            return Task.FromResult(DbContext.Set<T>().Where(dataLoadCondition).ToList());
        }

        /// <summary>
        /// Super Slow -> use only if neccessary!
        /// </summary>
        public async virtual Task<List<object>> GetDataAsync(Type type)
        {
            return await DbContext.Set(type).ToListAsync();
        }

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
            if (await DbContext.Set<T>().FindAsync(entry.GetPrimaryKeys()) == null)
                return false;

            DbContext.Set<T>().Remove(entry);

            return true;
        }


        public async virtual Task<int> SaveChangesAsync()
        {
            return await DbContext.SaveChangesAsync();
        }
    }
}
