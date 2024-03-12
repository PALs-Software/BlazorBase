using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class IncludeNavigationPropertyOnListLoadAttribute : Attribute
{
    public IncludeNavigationPropertyOnListLoadAttribute()
    {
    }

    public bool Include { get; set; } = true;
}
