using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BlazorBase.CRUD.Extensions
{
    public static class PropertyInfoExtension
    {
        public static bool HasAttribute(this PropertyInfo propertyInfo, Type type)
        {
            return Attribute.IsDefined(propertyInfo, type);
        }

        public static bool TryGetAttribute<T>(this PropertyInfo propertyInfo, [NotNullWhen(true)] out T? attribute) where T : Attribute
        {
            attribute = propertyInfo.GetCustomAttribute<T>();
            return attribute != null;
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

        public static bool IsVisibleInGUI(this PropertyInfo propertyInfo, GUIType? guiType = null)
        {
            bool isVisible;
            if (guiType == null)
                isVisible = propertyInfo.HasAttribute(typeof(VisibleAttribute));
            else
                isVisible = propertyInfo.GetCustomAttribute(typeof(VisibleAttribute)) is VisibleAttribute visible && !visible.HideInGUITypes.Contains(guiType.Value);

            if (isVisible && IsBaseModelDateProperty(propertyInfo))
                return !HideBaseModelDateProperties(propertyInfo, guiType);
            else
                return isVisible;
        }

        public static bool IsBaseModelDateProperty(this PropertyInfo propertyInfo)
        {
            return typeof(BaseModel).IsAssignableFrom(propertyInfo.ReflectedType) && (propertyInfo.Name == nameof(BaseModel.CreatedOn) || propertyInfo.Name == nameof(BaseModel.ModifiedOn));
        }

        public static bool HideBaseModelDateProperties(this PropertyInfo propertyInfo, GUIType? guiType = null)
        {
            return propertyInfo.ReflectedType?.GetCustomAttribute<HideBaseModelDatePropertiesInGUIAttribute>() is HideBaseModelDatePropertiesInGUIAttribute hideProperty &&
                (
                    hideProperty.HideCreatedOn && propertyInfo.Name == nameof(BaseModel.CreatedOn) ||
                    hideProperty.HideModifiedOn && propertyInfo.Name == nameof(BaseModel.ModifiedOn)
                ) && (guiType == null || hideProperty.HideInGUITypes.Contains(guiType.Value));
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

        public static (string DisplayPath, Type DisplayType) GetDisplayPropertyPathAndType(this PropertyInfo property, IServiceProvider serviceProvider)
        {
            var propertyRenderType = property.GetCustomAttribute<RenderTypeAttribute>()?.RenderType ?? property.PropertyType;
            var foreignKey = property.GetCustomAttribute<ForeignKeyAttribute>();
            if (foreignKey == null || property.IsListProperty())
                return (property.Name, propertyRenderType);

            var foreignProperty = property.ReflectedType?.GetProperties().Where(entry => entry.Name == foreignKey.Name).FirstOrDefault();
            if (foreignProperty == null)
                return (property.Name, propertyRenderType);

            var foreignKeyType = foreignProperty.GetCustomAttribute<RenderTypeAttribute>()?.RenderType ?? foreignProperty?.PropertyType;
            if (foreignKeyType == null)
                return (property.Name, propertyRenderType);

            if (!typeof(IBaseModel).IsAssignableFrom(foreignKeyType))
                return (property.Name, propertyRenderType);

            if (foreignKeyType.IsInterface)
            {
                var type = serviceProvider.GetService(foreignKeyType)?.GetType();
                if (type != null && typeof(IBaseModel).IsAssignableFrom(type))
                    foreignKeyType = type;
            }

            var displayKeyProperties = foreignKeyType.GetDisplayKeyProperties();
            if (displayKeyProperties.Count == 0)
                displayKeyProperties = foreignKeyType.GetKeyProperties();

            List<string> displayPropertyPaths = new();
            foreach (var displayKeyProperty in displayKeyProperties)
                displayPropertyPaths.Add($"{foreignKey.Name}.{displayKeyProperty.Name}");

            return (String.Join("|", displayPropertyPaths), displayKeyProperties[0].PropertyType);
        }

        public static bool GetPropertyIsSortAndFilterable(this PropertyInfo property)
        {
            if (property.HasAttribute(typeof(NotMappedAttribute)))
                return false;

            var getMethod = property.GetGetMethod();
            var setMethod = property.GetSetMethod();
            if (getMethod == null || setMethod == null)
                return false;

            //Check if get and set method are not overridden with custom logic and are just normal property get and set methods
            return Attribute.IsDefined(getMethod, typeof(CompilerGeneratedAttribute)) && Attribute.IsDefined(setMethod, typeof(CompilerGeneratedAttribute));
        }
    }
}
