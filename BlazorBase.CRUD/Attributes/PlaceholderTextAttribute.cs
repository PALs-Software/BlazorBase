using System;

namespace BlazorBase.CRUD.Attributes
{
    public class PlaceholderTextAttribute : Attribute
    {
        public PlaceholderTextAttribute(string placeholder)
        {
            Placeholder = placeholder;
        }
        public string? Placeholder { get; set; }
    }
}
