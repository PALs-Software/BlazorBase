using BlazorBase.CRUD.Enums;
using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class HideBaseModelDatePropertiesInGUIAttribute : Attribute
{
    public bool HideCreatedOn { get; set; } = true;
    public bool HideModifiedOn { get; set; } = true;

    public GUIType[] HideInGUITypes { get; set; } = new GUIType[] { GUIType.List, GUIType.ListPart, GUIType.Card };
}
