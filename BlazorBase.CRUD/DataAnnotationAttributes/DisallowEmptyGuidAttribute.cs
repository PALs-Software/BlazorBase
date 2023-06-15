using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorBase.CRUD.DataAnnotationAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DisallowEmptyGuidAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value != null && value is Guid guid && guid != Guid.Empty;
    }
}
