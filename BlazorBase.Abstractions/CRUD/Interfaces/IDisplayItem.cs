using BlazorBase.Abstractions.CRUD.Attributes;
using BlazorBase.Abstractions.CRUD.Enums;
using System.Reflection;

namespace BlazorBase.Abstractions.CRUD.Interfaces;

public interface IDisplayItem
{
    #region Customization Attributes

    Dictionary<CustomizationLocation, string> CustomizationClasses { get; }
    Dictionary<CustomizationLocation, string> CustomizationStyles { get; }

    #endregion

    PropertyInfo Property { get; set; }
    VisibleAttribute Attribute { get; set; }
    GUIType GUIType { get; set; }
    bool IsReadOnly { get; set; }
    bool IsKey { get; set; }
    bool IsListProperty { get; set; }
    bool IsPrimitiveListType { get; set; }
    PresentationDataType? PresentationDataType { get; set; }
    SortDirection SortDirection { get; set; }
    FilterType FilterType { get; set; }
    object? FilterValue { get; set; }
    string? DisplayPropertyPath { get; set; }
    Type DisplayPropertyType { get; set; }
    bool IsSortable { get; set; }
    bool IsFilterable { get; set; }
}
