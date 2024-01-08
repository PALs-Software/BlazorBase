using BlazorBase.CRUD.Enums;
using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class OnlyEditableForRolesAttribute : Attribute
{
    public OnlyEditableForRolesAttribute() { }

    public OnlyEditableForRolesAttribute(GUIType guiType, params string[] roles)
    {
        GUITypes = new GUIType[] { guiType };
        Roles = roles;
    }

    public OnlyEditableForRolesAttribute(GUIType[] guiTypes, params string[] roles)
    {
        GUITypes = guiTypes;
        Roles = roles;
    }

    public OnlyEditableForRolesAttribute(params string[] roles)
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
