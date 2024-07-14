using BlazorBase.Abstractions.CRUD.Enums;

namespace BlazorBase.Abstractions.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class HideBaseModelDatePropertiesInGUIAttribute : Attribute
{
    public bool HideCreatedOn { get; set; } = true;
    public bool HideModifiedOn { get; set; } = true;

    public GUIType[] HideInGUITypes { get; set; } = [GUIType.List, GUIType.ListPart, GUIType.Card];
}
