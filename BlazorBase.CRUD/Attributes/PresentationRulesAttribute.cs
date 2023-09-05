using BlazorBase.CRUD.Enums;
using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class PresentationRulesAttribute : Attribute
{
    public PresentationRulesAttribute() {}

    public PresentationRulesAttribute(params PresentationRule[] rules )
    {
        Rules = rules;
    }

    public PresentationRule[] Rules { get; set; } = Array.Empty<PresentationRule>();
}
