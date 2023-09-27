using BlazorBase.CRUD.Enums;
using System;
using System.Linq;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class CustomClassAndStyleAttribute : BaseCustomizationAttribute
{
    public string? Class { get; set; }

    public string? Style { get; set; }

    public CustomizationLocation[]? Locations { get; set; }

    public override string GetClass(GUIType guiType, CustomizationLocation location)
    {
        if (Locations != null && !Locations.Contains(location))
            return String.Empty;

        return Class ?? String.Empty;
    }

    public override string GetStyle(GUIType guiType, CustomizationLocation location)
    {
        if (Locations != null && !Locations.Contains(location))
            return String.Empty;

        return Style ?? String.Empty;
    }
}