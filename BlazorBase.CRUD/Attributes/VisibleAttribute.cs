﻿using BlazorBase.CRUD.Enums;
using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class VisibleAttribute : Attribute
{
    public VisibleAttribute() { }

    public VisibleAttribute(string displaygroup = "", int displayGroupOrder = 0, int displayOrder = 0, bool collapsed = false, SortDirection sortDirection = SortDirection.None, int sortOrder = 0, params GUIType[] hideInGUITypes)
    {
        HideInGUITypes = hideInGUITypes;
        DisplayGroupOrder = displayGroupOrder;
        DisplayGroup = displaygroup;
        DisplayOrder = displayOrder;
        Collapsed = collapsed;
        SortDirection = sortDirection;
        SortOrder = sortOrder;
    }

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

    public SortDirection SortDirection { get; set; }

    public int SortOrder { get; set; }
}
