using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class BaseListPartDisplayOptionsAttribute : Attribute
{
    public BaseListPartDisplayOptionsAttribute() { }

    public BaseListPartDisplayOptionsAttribute(bool showAddButton, bool showAddExistingEntryButton, bool showDeleteButton)
    {
        ShowAddButton = showAddButton;
        ShowAddExistingEntryButton = showAddExistingEntryButton;
        ShowDeleteButton = showDeleteButton;
    }

    public bool ShowAddButton { get; set; } = true;
    public bool ShowAddExistingEntryButton { get; set; } = true;
    public bool ShowCardEditButton { get; set; } = false;
    public bool ShowDeleteButton { get; set; } = true;
}
