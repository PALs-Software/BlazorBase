using BlazorBase.CRUD.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Models.IBaseModel;

namespace BlazorBase.CRUD.Extensions
{
    public static class DbContextExtension
    {
        /// <summary>
        /// Super Slow -> use only if neccessary!
        /// </summary>
        public static IQueryable<object> Set(this DbContext context, Type type)
        {
            return (IQueryable<object>)context.GetType().GetMethods().Single(method => method.Name == "Set" && method.GetParameters().Length == 0).MakeGenericMethod(type).Invoke(context, null);
        }

        public static void HandleBlazorBaseCRUDEvents(this DbContext context, IServiceProvider serviceProvider)
        {
            Task.Run(async () =>
            {
                context.ChangeTracker.DetectChanges();

                var markedAsDeleted = context.ChangeTracker.Entries().Where(x => x.State == EntityState.Deleted);
                var markedAsModified = context.ChangeTracker.Entries().Where(x => x.State == EntityState.Modified);
                var markedAsAdded = context.ChangeTracker.Entries().Where(x => x.State == EntityState.Added);

                var addArgs = new OnDbContextAddEntryArgs(context, serviceProvider);
                foreach (var item in markedAsAdded)
                    if (item.Entity is IBaseModel model)
                        await model.OnDbContextAddEntry(addArgs);

                var modifyArgs = new OnDbContextModifyEntryArgs(context, serviceProvider);
                foreach (var item in markedAsModified)
                    if (item.Entity is IBaseModel model)
                        await model.OnDbContextModifyEntry(modifyArgs);

                var deleteArgs = new OnDbContextDeleteEntryArgs(context, serviceProvider);
                foreach (var item in markedAsDeleted)
                    if (item.Entity is IBaseModel model)
                        await model.OnDbContextDeleteEntry(deleteArgs);

            }).Wait();
        }
    }
}
