using BlazorBase.Abstractions.CRUD.Enums;
using System;

namespace BlazorBase.CRUD.Attributes;

public abstract class BaseCustomizationAttribute : Attribute
{
    public GUIType[] ValidInGUITypes { get; set; } = [
        GUIType.List,
        GUIType.ListPart,
        GUIType.Card
    ];

    public abstract string GetClass(GUIType guiType, CustomizationLocation location);

    public abstract string GetStyle(GUIType guiType, CustomizationLocation location);
}