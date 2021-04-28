using Blazorise;
using System;

namespace BlazorBase.CRUD.Attributes
{
    public class DateDisplayModeAttribute : Attribute
    {
        public DateInputMode DateInputMode { get; set; }
    }
}
