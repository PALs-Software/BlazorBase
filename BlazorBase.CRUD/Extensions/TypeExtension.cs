using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace BlazorBase.CRUD.Extensions
{
    public static class TypeExtension
    {
        public static List<PropertyInfo> GetKeyProperties(this Type type)
        {
            return type.GetProperties().Where(property =>
                        (!property.PropertyType.IsSubclassOf(typeof(IBaseModel))) &&
                        property.IsKey()
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

        public static PropertyInfo GetDisplayKeyProperty(this Type type)
        {
            return type.GetProperties().Where(property => property.IsDisplayKey()).FirstOrDefault();
        }
    }
}
