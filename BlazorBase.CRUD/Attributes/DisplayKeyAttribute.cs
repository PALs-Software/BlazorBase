using System;

namespace BlazorBase.CRUD.Attributes
{
    public class DisplayKeyAttribute : Attribute
    {
        public int DisplayOrder { get; set; }
    }
}
