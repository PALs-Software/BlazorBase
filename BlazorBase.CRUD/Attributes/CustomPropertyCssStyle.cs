using System;

namespace BlazorBase.CRUD.Attributes;

public class CustomPropertyCssStyleAttribute : Attribute
{
    public string? PropertyStyle { get; set; }
    public string? InputStyle { get; set; }    
}
