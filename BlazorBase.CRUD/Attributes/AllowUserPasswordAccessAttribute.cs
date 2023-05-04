using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class AllowUserPasswordAccessAttribute : Attribute
{
    public string? AllowAccessCallbackMethodName { get; set; }
}
