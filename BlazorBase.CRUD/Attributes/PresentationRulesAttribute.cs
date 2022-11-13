using BlazorBase.CRUD.Enums;
using System;

namespace BlazorBase.CRUD.Attributes
{
    public class PresentationRulesAttribute : Attribute
    {
        public PresentationRulesAttribute() {}

        public PresentationRulesAttribute(params PresentationRule[] rules )
        {
            Rules = rules;
        }

        public PresentationRule[] Rules { get; set; } = Array.Empty<PresentationRule>();
    }
}
