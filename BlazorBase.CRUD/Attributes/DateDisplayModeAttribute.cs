using Blazorise;
using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class DateDisplayModeAttribute : Attribute
{
    public DateInputMode DateInputMode { get; set; }
}
