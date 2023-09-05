using BlazorBase.CRUD.Enums;
using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class StickyColumnAttribute : BaseCustomizationAttribute
{
    public string? Left { get; set; }

    public string? MinWidth { get; set; } = null;

    public override string GetClass(GUIType guiType, CustomizationLocation location)
    {
        if (guiType != GUIType.List)
            return String.Empty;

        return "base-list-sticky-column";
    }

    public override string GetStyle(GUIType guiType, CustomizationLocation location)
    {
        if (guiType != GUIType.List)
            return String.Empty;

        var style = $"left:{Left};";
        if (MinWidth != null)
            style += $"min-width: {MinWidth};";

        return style;
    }
}