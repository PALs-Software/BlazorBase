using BlazorBase.CRUD.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HideBaseModelDatePropertiesInGUIAttribute : Attribute
    {
        public bool HideCreatedOn { get; set; } = true;
        public bool HideModifiedOn { get; set; } = true;

        public GUIType[] HideInGUITypes { get; set; } = new GUIType[] { GUIType.List, GUIType.ListPart, GUIType.Card };
    }
}
