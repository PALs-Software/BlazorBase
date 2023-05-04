using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class UseCustomLookupData : Attribute
{
    public UseCustomLookupData(string lookupDataSourceMethodName) => LookupDataSourceMethodName = lookupDataSourceMethodName;
    public string LookupDataSourceMethodName { get; set; }
}
