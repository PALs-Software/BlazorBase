using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;
using System.Reflection;

namespace BlazorBase.Extensions;
public static class ObjectExtension
{
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
