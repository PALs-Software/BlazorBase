using BlazorBase.Abstractions.CRUD.Enums;
using System;

namespace BlazorBase.Files.Attributes;

public class HideShowFileButtonAttribute : Attribute
{
    public HideShowFileButtonAttribute() {
        HideInGUITypes = [
            GUIType.List,
            GUIType.ListPart,
            GUIType.Card
        ];
    }

    public HideShowFileButtonAttribute(params GUIType[] onlyHideInGUITypes)
    {
        HideInGUITypes = onlyHideInGUITypes;          
    }

    public GUIType[] HideInGUITypes { get; set; } = Array.Empty<GUIType>();     
}
