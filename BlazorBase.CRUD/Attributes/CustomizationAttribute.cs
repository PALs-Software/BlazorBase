using BlazorBase.CRUD.Enums;
using System;

namespace BlazorBase.CRUD.Attributes;

public abstract class CustomizationAttribute : Attribute
{
    public GUIType[] ValidInGUITypes { get; set; } = new GUIType[] {
        GUIType.List,
        GUIType.ListPart,
        GUIType.Card
    };

    public abstract string GetClass(GUIType guiType);

    public abstract string GetStyle(GUIType guiType);
}
