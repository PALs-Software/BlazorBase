using BlazorBase.CRUD.Enums;
using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class VisibleAttribute : Attribute
{
    public VisibleAttribute() { }

    public VisibleAttribute(bool hideInGUI)
    {
        if (!hideInGUI)
            return;

        HideInGUITypes = [
            GUIType.List,
            GUIType.ListPart,
            GUIType.Card
        ];
    }

    public GUIType[] HideInGUITypes { get; set; } = [];

    public string? DisplayGroup { get; set; }

    public int DisplayGroupOrder { get; set; }

    public int DisplayOrder { get; set; }

    public bool Collapsed { get; set; }

    public bool IsFilterable { get; set; } = true;

    public bool IsSortable { get; set; } = true;
    public SortDirection SortDirection { get; set; }
    public int SortOrder { get; set; }
}
