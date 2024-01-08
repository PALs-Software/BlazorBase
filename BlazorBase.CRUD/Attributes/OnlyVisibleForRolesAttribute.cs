using BlazorBase.CRUD.Enums;
using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class OnlyVisibleForRolesAttribute : Attribute
{
    public OnlyVisibleForRolesAttribute() { }

    public OnlyVisibleForRolesAttribute(GUIType guiType, params string[] roles)
    {
        GUITypes = new GUIType[] { guiType };
        Roles = roles;
    }

    public OnlyVisibleForRolesAttribute(GUIType[] guiTypes, params string[] roles)
    {
        GUITypes = guiTypes;
        Roles = roles;
    }

    public OnlyVisibleForRolesAttribute(params string[] roles)
    {
        Roles = roles;
    }

    public string[] Roles { get; set; } = Array.Empty<string>();

    public GUIType[] GUITypes { get; set; } = new GUIType[]
    {
        GUIType.List,
        GUIType.ListPart,
        GUIType.Card
    };
}
