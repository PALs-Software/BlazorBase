using System;

namespace BlazorBase.Files.Attributes
{
    public class FileInputFilterAttribute : Attribute
    {
        public string? Filter { get; set; }
    }
}
