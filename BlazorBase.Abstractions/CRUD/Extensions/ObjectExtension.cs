using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Reflection;

namespace BlazorBase.Abstractions.CRUD.Extensions;
public static class ObjectExtension
{
    public static void TransferPropertiesTo<T>(this T source, object target, params string[] exceptPropertyNames) where T : class
    {
        var sourceProperties = source.GetType().GetProperties().Where(property => !exceptPropertyNames.Contains(property.Name));
        TransferPropertiesTo(source, target, sourceProperties.ToArray());
    }

    public static void TransferPropertiesTo(this object source, object target, PropertyInfo[]? sourceProperties = null)
    {
        if (sourceProperties == null)
            sourceProperties = source.GetType().GetProperties();
        var targetProperties = target.GetType().GetProperties();

        foreach (var sourceProperty in sourceProperties)
        {
            var targetProperty = targetProperties.Where(entry => entry.Name == sourceProperty.Name).FirstOrDefault();

            if (targetProperty == null ||
                (!sourceProperty.CanRead || !targetProperty.CanWrite) ||
                (!targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType)) ||
                (targetProperty.GetSetMethod() == null) ||
                ((targetProperty.GetSetMethod()?.Attributes & MethodAttributes.Static) != 0) ||
                typeof(ILazyLoader).IsAssignableFrom(sourceProperty.PropertyType))
                continue;

            targetProperty.SetValue(target, sourceProperty.GetValue(source));
        }
    }
}
