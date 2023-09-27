using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class DisplayKeyAttribute : Attribute
{
    public int DisplayOrder { get; set; }
}
