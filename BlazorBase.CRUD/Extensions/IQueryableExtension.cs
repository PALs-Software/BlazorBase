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
        static Expression EmptyGuid = Expression.Constant(Guid.Empty);

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
            if (filterValue != null)
            {
                if (filterValue is DateTime dateTime && (displayItem.FilterType == FilterType.Like || displayItem.FilterType == FilterType.Equal))
                {
                    if (displayItem.DateInputMode == DateInputMode.Date)
                        filterValue = dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    else
                        filterValue = dateTime.ToString("yyyy-MM-dd hh:mm", CultureInfo.InvariantCulture);
                }
                else if (filterValue is string || filterValue is Guid || filterValue is Guid?)
                    filterValue = filterValue?.ToString()?.Replace(" ", "%");
            }

            var parameter = Expression.Parameter(typeof(TModel));
            var property = Expression.Property(parameter, propertyName);
            ConstantExpression constant;
            if (filterValue is string || displayItem.Property.PropertyType == typeof(Guid) || displayItem.Property.PropertyType == typeof(Guid?))
                constant = Expression.Constant(filterValue);
            else
                constant = Expression.Constant(filterValue, displayItem.Property.PropertyType);

            Expression body;
            switch (displayItem.FilterType)
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
                    if (displayItem.Property.PropertyType == typeof(Guid) || displayItem.Property.PropertyType == typeof(Guid?))
                        body = Expression.Equal(property, EmptyGuid);
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
