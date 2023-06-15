using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class PlaceholderTextAttribute : Attribute
{
    public PlaceholderTextAttribute(string placeholder)
    {
        Placeholder = placeholder;
    }
    public string Placeholder { get; set; }
}
