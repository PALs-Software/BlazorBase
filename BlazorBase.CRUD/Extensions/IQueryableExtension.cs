using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Helper;
using Blazorise;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Components.BaseDisplayComponent;

namespace BlazorBase.CRUD.Extensions
{
    public static class IQueryableExtension
    {
        static MethodInfo LikeMethodInfo = typeof(DbFunctionsExtensions).GetMethod("Like", new Type[] { typeof(DbFunctions), typeof(string), typeof(string) });
        static Expression EFFunctionsConstant = Expression.Constant(EF.Functions);
        static Expression EmptyStringConstant = Expression.Constant(String.Empty);
        static Expression NullConstant = Expression.Constant(null);
        static Expression EmptyGuid = Expression.Constant(Guid.Empty, typeof(Guid));
        static Expression EmptyNullableGuid = Expression.Constant(Guid.Empty, typeof(Guid?));

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderBy(ToLambda<T>(propertyName));
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderByDescending(ToLambda<T>(propertyName));
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string propertyName)
        {
            return source.ThenBy(ToLambda<T>(propertyName));
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string propertyName)
        {
            return source.ThenByDescending(ToLambda<T>(propertyName));
        }

        private static Expression<Func<T, object>> ToLambda<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T));
            var property = Expression.Property(parameter, propertyName);
            var propAsObject = Expression.Convert(property, typeof(object));

            return Expression.Lambda<Func<T, object>>(propAsObject, parameter);
        }

        public static IQueryable<T> Where<T>(this IQueryable<T> source, DisplayItem displayItem)
        {
            if (displayItem.FilterType != FilterType.IsNull && displayItem.FilterType != FilterType.IsEmpty && String.IsNullOrEmpty(displayItem.FilterValue?.ToString()))
                return source;
            return source.Where(CreateFilterExpression<T>(displayItem));
        }

        private static Expression<Func<TModel, bool>> CreateFilterExpression<TModel>(DisplayItem displayItem)
        {
            var propertyName = displayItem.Property.Name;
            var filterValue = displayItem.FilterValue;
            var filterType = displayItem.FilterType;
            if (filterValue != null)
            {
                if (filterValue is DateTime dateTime)
                {
                    if (filterType == FilterType.Equal)
                    {
                        filterType = FilterType.Like;
                        if (displayItem.DateInputMode == DateInputMode.Date)
                            filterValue = dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        else
                            filterValue = dateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    }
                    else if (filterType == FilterType.Greater || filterType == FilterType.LessOrEqual)
                    {
                        if (displayItem.DateInputMode == DateInputMode.Date)
                            filterValue = dateTime.Date + new TimeSpan(0, 23, 59, 59, 999);
                        else
                            filterValue = dateTime.Date + new TimeSpan(0, dateTime.Hour, dateTime.Minute, dateTime.Second, 999);
                    }

                }
                else if ((filterType == FilterType.Like) && (filterValue is string || filterValue is Guid || filterValue is Guid?))
                    filterValue = filterValue?.ToString()?.Replace(" ", "%");
            }

            ConstantExpression constant = null;
            Expression body;
            var parameter = Expression.Parameter(typeof(TModel));
            var property = Expression.Property(parameter, propertyName);

            if ((filterType == FilterType.Like) && (filterValue is string || displayItem.Property.PropertyType == typeof(Guid) || displayItem.Property.PropertyType == typeof(Guid?)))
                constant = Expression.Constant(filterValue);
            else if (displayItem.Property.PropertyType != typeof(Guid) && displayItem.Property.PropertyType != typeof(Guid?))
                constant = Expression.Constant(filterValue, displayItem.Property.PropertyType);

            switch (filterType)
            {
                case FilterType.Like:
                    if (TypeHelper.NumericTypes.Contains(displayItem.Property.PropertyType))
                    {
                        var savedCulture = Thread.CurrentThread.CurrentCulture;
                        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                        filterValue = filterValue.ToString();
                        Thread.CurrentThread.CurrentCulture = savedCulture;
                    }

                    var propertyAsObject = Expression.Convert(property, typeof(object));
                    var propertyAsString = Expression.Convert(propertyAsObject, typeof(string));
                    var filter = Expression.Constant($"%{filterValue}%");
                    body = Expression.Call(LikeMethodInfo, EFFunctionsConstant, propertyAsString, filter);
                    break;
                case FilterType.Equal:
                    body = Expression.Equal(property, constant);
                    break;
                case FilterType.Greater:
                    body = Expression.GreaterThan(property, constant);
                    break;
                case FilterType.GreaterOrEqual:
                    body = Expression.GreaterThanOrEqual(property, constant);
                    break;
                case FilterType.Less:
                    body = Expression.LessThan(property, constant);
                    break;
                case FilterType.LessOrEqual:
                    body = Expression.LessThanOrEqual(property, constant);
                    break;
                case FilterType.IsEmpty:
                    if (displayItem.Property.PropertyType == typeof(Guid))
                        body = Expression.Equal(property, EmptyGuid);
                    else if (displayItem.Property.PropertyType == typeof(Guid?))
                        body = Expression.Equal(property, EmptyNullableGuid);
                    else
                        body = Expression.Equal(property, EmptyStringConstant);
                    break;
                case FilterType.IsNull:
                    body = Expression.Equal(property, NullConstant);
                    break;
                default:
                    throw new Exception($"The Filter Type {displayItem.FilterType} is not supported");
            }

            return Expression.Lambda<Func<TModel, bool>>(body, parameter);
        }
    }
}
