using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.SortableItem;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace BlazorBase.CRUD.Extensions
{
    public static class TypeExtension
    {
        public static Type GetUnproxiedType(this Type type)
        {
            if (type.Namespace == "Castle.Proxies")
                return type.BaseType;

            return type;
        }

        public static List<PropertyInfo> GetKeyProperties(this Type type)
        {
            return type.GetProperties().Where(property =>
                        (!typeof(IBaseModel).IsAssignableFrom(property.PropertyType)) &&
                        property.IsKey()
                    ).OrderBy(entry => entry.GetAttribute<ColumnAttribute>()?.Order ?? 0).ToList();
        }

        public static List<PropertyInfo> GetPropertiesExceptKeys(this Type type)
        {
            return type.GetProperties().Where(property =>
                        !property.IsKey()
                    ).ToList();
        }

        public static List<PropertyInfo> GetVisibleProperties(this Type type)
        {
            return type.GetProperties().Where(entry => entry.IsVisibleInGUI()).ToList();
        }

        public static List<PropertyInfo> GetVisibleProperties(this Type type, GUIType guiType)
        {
            return type.GetProperties().Where(entry => entry.IsVisibleInGUI(guiType)).ToList();
        }

        public static List<PropertyInfo> GetDisplayKeyProperties(this Type type)
        {
            var properties = type.GetProperties().Where(property => property.IsDisplayKey()).ToList();
            var orderDictionary = new Dictionary<PropertyInfo, DisplayKeyAttribute>();
            foreach (var property in properties)
                orderDictionary.Add(property, property.GetCustomAttributes(typeof(DisplayKeyAttribute)).First() as DisplayKeyAttribute);

            return orderDictionary.OrderBy(entry => entry.Value.DisplayOrder).Select(entry => entry.Key).ToList();
        }

        public static List<PropertyInfo> GetIBaseModelProperties(this Type type)
        {
            return type.GetProperties().Where(property =>
                typeof(IBaseModel).IsAssignableFrom(property.PropertyType) ||
                 (property.PropertyType.IsGenericType && typeof(IBaseModel).IsAssignableFrom(property.PropertyType.GenericTypeArguments[0]))
            ).ToList();
        }

        public static bool ImplementedISortableItem(this Type type)
        {
            return typeof(ISortableItem).IsAssignableFrom(type);
        }
    }
}
