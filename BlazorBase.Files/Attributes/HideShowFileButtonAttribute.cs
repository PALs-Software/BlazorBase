using BlazorBase.CRUD.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Components.BaseDisplayComponent;

namespace BlazorBase.Files.Attributes
{
    public class HideShowFileButtonAttribute : Attribute
    {
        public HideShowFileButtonAttribute() {
            HideInGUITypes = new GUIType[] {
                GUIType.List,
                GUIType.ListPart,
                GUIType.Card
            };
        }

        public HideShowFileButtonAttribute(params GUIType[] onlyHideInGUITypes)
        {
            HideInGUITypes = onlyHideInGUITypes;          
        }

        public GUIType[] HideInGUITypes { get; set; } = Array.Empty<GUIType>();     
    }
}
