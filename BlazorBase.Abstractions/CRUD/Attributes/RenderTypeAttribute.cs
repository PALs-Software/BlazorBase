namespace BlazorBase.Abstractions.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class RenderTypeAttribute : Attribute
{
    public RenderTypeAttribute(Type renderType)
    {
        RenderType = renderType;
    }

    public Type RenderType { get; set; }
}
