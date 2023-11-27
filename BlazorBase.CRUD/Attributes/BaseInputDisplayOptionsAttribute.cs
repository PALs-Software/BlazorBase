using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class BaseInputDisplayOptionsAttribute : Attribute
{ 
    /// <summary>
    /// Determines whether the select existing entity in the base inputs by foreign keys is displayed or not
    /// </summary>
    public bool ShowSelectExistingEntityButton { get; set; } = true;
}
