using BlazorBase.Abstractions.CRUD.Enums;

namespace BlazorBase.Abstractions.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class PresentationDataTypeAttribute : Attribute
{
    public PresentationDataTypeAttribute(PresentationDataType type)
    {
        Type = type;
    }

    public PresentationDataType Type { get; set; }
}
