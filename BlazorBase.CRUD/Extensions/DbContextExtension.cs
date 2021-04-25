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
    }
}
