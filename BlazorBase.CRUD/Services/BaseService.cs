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
        readonly DbContext DbContext;

        public BaseService(DbContext context)
        {
            DbContext = context;
        }

        public async Task<List<T>> GetDataAsync<T>() where T : class
        {
            return await DbContext.Set<T>().ToListAsync();
        }

        /// <summary>
        /// Super Slow -> use only if neccessary!
        /// </summary>
        public async Task<List<object>> GetDataAsync(Type type)
        {
            return await DbContext.Set(type).ToListAsync();
        }

        public async Task ReloadAsync<T>(T entry) where T : class
        {
            await DbContext.Entry(entry).ReloadAsync();
        }

        public async Task<bool> AddEntry<T>(T entry) where T : BaseModel
        {
            if (entry == null)
                return false;

            if (await DbContext.Set<T>().FindAsync(entry.GetPrimaryKeys()) != null)
                return false;

            DbContext.Set<T>().Add(entry);

            return true;
        }

        public void UpdateEntry<T>(T entry) where T : class
        {
            DbContext.Set<T>().Update(entry);
        }

        public async Task<bool> RemoveEntryAsync<T>(T entry) where T : BaseModel
        {
            if (await DbContext.Set<T>().FindAsync(entry.GetPrimaryKeys()) == null)
                return false;

            DbContext.Set<T>().Remove(entry);

            return true;
        }


        public async Task SaveChangesAsync()
        {
            await DbContext.SaveChangesAsync();
        }
    }
}
