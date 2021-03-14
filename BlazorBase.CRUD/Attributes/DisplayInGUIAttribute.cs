using BlazorBase.CRUD.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Attributes
{
    public class VisibleAttribute : Attribute
    {
        public VisibleAttribute(params GUIType[] hideInGUITypes)
        {
            HideInGUITypes = hideInGUITypes.ToList(); 
        }

        public List<GUIType> HideInGUITypes { get; set; }
    }
}
