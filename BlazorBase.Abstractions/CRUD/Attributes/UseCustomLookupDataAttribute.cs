namespace BlazorBase.Abstractions.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class UseCustomLookupDataAttribute : Attribute
{
    public UseCustomLookupDataAttribute(string lookupDataSourceMethodName) => LookupDataSourceMethodName = lookupDataSourceMethodName;
    public string LookupDataSourceMethodName { get; set; }
}
