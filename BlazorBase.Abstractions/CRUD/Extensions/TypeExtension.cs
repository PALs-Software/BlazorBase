using BlazorBase.Abstractions.CRUD.Attributes;
using BlazorBase.Abstractions.CRUD.Enums;
using BlazorBase.Abstractions.CRUD.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace BlazorBase.Abstractions.CRUD.Extensions;

public static class TypeExtension
{
    public static Type GetUnproxiedType(this Type type)
    {
        if (type.Namespace == "Castle.Proxies" && type.BaseType != null)
            return type.BaseType;

        return type;
    }

    public static List<PropertyInfo> GetKeyProperties(this Type type)
    {
        return type.GetProperties().Where(property =>
                    (!typeof(IBaseModel).IsAssignableFrom(property.PropertyType)) &&
                    property.IsKey()
                ).OrderBy(entry => entry.GetCustomAttribute<ColumnAttribute>()?.Order ?? 0).ToList();
    }

    public static List<PropertyInfo> GetPropertiesExceptKeys(this Type type)
    {
        return type.GetProperties().Where(property =>
                    !property.IsKey()
                ).ToList();
    }

    public static List<PropertyInfo> GetVisibleProperties(this Type type, GUIType? guiType = null, List<string>? userRoles = null)
    {
        return type.GetProperties().Where(entry => entry.IsVisibleInGUI(guiType, userRoles)).ToList();
    }

    public static List<PropertyInfo> GetDisplayKeyProperties(this Type type)
    {
        var properties = type.GetProperties().Where(property => property.IsDisplayKey()).ToList();
        var orderDictionary = new Dictionary<PropertyInfo, DisplayKeyAttribute>();
        foreach (var property in properties)
            orderDictionary.Add(property, (DisplayKeyAttribute)property.GetCustomAttributes(typeof(DisplayKeyAttribute)).First());

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
