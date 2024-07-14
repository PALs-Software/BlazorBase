﻿using BlazorBase.Abstractions.CRUD.Enums;
using System;

namespace BlazorBase.Abstractions.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class DefaultListFilterAttribute : Attribute
{
    public DefaultListFilterAttribute(){}

    public DefaultListFilterAttribute(FilterType type, object? value)
    {
        Type = type;
        Value = value;
    }

    public FilterType Type { get; set; }
    public object? Value { get; set; }
}
