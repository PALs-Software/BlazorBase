using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Attributes
{
    public class TextDisplayModeAttribute : Attribute
    {
        public TextDisplayMode Mode { get; set; }
    }

    public enum TextDisplayMode
    {
        Text,
        Multiline
    }
}
