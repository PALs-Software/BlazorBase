using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Attributes
{
    public class RenderTypeAttribute : Attribute
    {
        public RenderTypeAttribute() { }
        public RenderTypeAttribute(Type renderType)
        {
            RenderType = renderType;
        }


        public Type RenderType { get; set; }
    }

}
