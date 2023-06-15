using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class CustomPropertyCssStyleAttribute : Attribute
{
    public string? PropertyStyle { get; set; }
    public string? InputStyle { get; set; }    
}
