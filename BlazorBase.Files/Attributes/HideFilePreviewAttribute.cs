using BlazorBase.CRUD.Enums;
using System;

namespace BlazorBase.Files.Attributes;

public class HideFilePreviewAttribute : Attribute
{
    public HideFilePreviewAttribute() {
        HideInGUITypes = [
            GUIType.List,
            GUIType.ListPart,
            GUIType.Card
        ];
    }

    public HideFilePreviewAttribute(params GUIType[] onlyHideInGUITypes)
    {
        HideInGUITypes = onlyHideInGUITypes;          
    }

    public GUIType[] HideInGUITypes { get; set; } = [];     
}
