using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class CustomSortAndFilterPropertyPathAttribute : Attribute
{
    public CustomSortAndFilterPropertyPathAttribute(string path, Type pathType)
    {
        Path = path;
        PathType = pathType;
    }

    public string Path { get; set; } = null!;
    public Type PathType { get; set; }
}
