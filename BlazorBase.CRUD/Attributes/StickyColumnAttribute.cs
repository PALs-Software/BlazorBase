using BlazorBase.CRUD.Enums;
using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class StickyColumnAttribute : CustomizationAttribute
{
    public StickyColumnAttribute(string left)
    {
        Left = left;
    }

    public string Left { get; set; }

    public string? MinWidth { get; set; } = null;

    public override string GetClass(GUIType guiType)
    {
        return "base-list-sticky-column";
    }

    public override string GetStyle(GUIType guiType)
    {
        var style = $"left:{Left};";
        if (MinWidth != null)
            style += $"min-width: {MinWidth};";

        return style;
    }
}