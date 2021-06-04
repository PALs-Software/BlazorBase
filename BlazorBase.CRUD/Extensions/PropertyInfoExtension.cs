using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace BlazorBase.CRUD.Extensions
{
    public static class PropertyInfoExtension
    {
       
        public static string GetPlaceholderText(this PropertyInfo propertyInfo)
        {
            if (Attribute.IsDefined(propertyInfo, typeof(PlaceholderTextAttribute)))
                return (propertyInfo.GetCustomAttribute(typeof(PlaceholderTextAttribute)) as PlaceholderTextAttribute).Placeholder;
            else
                return String.Empty;
        }

        public static bool HasAttribute(this PropertyInfo propertyInfo, Type type)
        {
            return Attribute.IsDefined(propertyInfo, type);
        }

        public static bool TryGetAttribute<T>(this PropertyInfo propertyInfo, out T attribute) where T : class
        {
            if (propertyInfo.HasAttribute(typeof(T)))
            {
                attribute = propertyInfo.GetCustomAttribute(typeof(T)) as T;
                return true;
            }
            else
            {
                attribute = default;
                return false;
            }
        }

        public static bool IsKey(this PropertyInfo propertyInfo)
        {
            return propertyInfo.HasAttribute(typeof(KeyAttribute));
        }

        public static bool IsForeignKey(this PropertyInfo propertyInfo)
        {
            return propertyInfo.HasAttribute(typeof(ForeignKeyAttribute));
        }

        public static bool UsesCustomLookupData(this PropertyInfo propertyInfo)
        {
            return propertyInfo.HasAttribute(typeof(UseCustomLookupData));
        }


        public static bool IsDisplayKey(this PropertyInfo propertyInfo)
        {
            return propertyInfo.HasAttribute(typeof(DisplayKeyAttribute));
        }

        public static bool IsVisibleInGUI(this PropertyInfo propertyInfo)
        {
            return !propertyInfo.HasAttribute(typeof(VisibleAttribute));
        }

        public static bool IsVisibleInGUI(this PropertyInfo propertyInfo, GUIType guiType)
        {
            return propertyInfo.GetCustomAttribute(typeof(VisibleAttribute)) is VisibleAttribute visible && !visible.HideInGUITypes.Contains(guiType);
        }

        public static bool IsReadOnlyInGUI(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute(typeof(EditableAttribute)) is EditableAttribute editable && !editable.AllowEdit;
        }

        public static bool IsListProperty(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                return false;

            return propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }
    }
}
