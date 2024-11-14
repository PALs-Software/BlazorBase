using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class HideListFilterTypesAttribute : Attribute
{
    public HideListFilterTypesAttribute(){}

    public bool Hide { get; set; } = true;
}
