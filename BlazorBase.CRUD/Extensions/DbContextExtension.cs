using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;

namespace BlazorBase.CRUD.Extensions
{
    public static class DbContextExtension
    {
        static MethodInfo SetMethodInfo = typeof(DbContext).GetMethods().Single(method => method.Name == "Set" && method.GetParameters().Length == 0);

        /// <summary>
        /// Very Slow, because method is created per reflection -> use only if neccessary!
        /// </summary>
        public static IQueryable<object> Set(this DbContext context, Type type)
        {
            return (IQueryable<object>)SetMethodInfo.MakeGenericMethod(type).Invoke(context, null)!;
        }
    }
}
