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
    public class HideDownloadFileButtonAttribute : Attribute
    {
        public HideDownloadFileButtonAttribute() {
            HideInGUITypes = new GUIType[] {
                GUIType.List,
                GUIType.ListPart,
                GUIType.Card
            };
        }

        public HideDownloadFileButtonAttribute(params GUIType[] onlyHideInGUITypes)
        {
            HideInGUITypes = onlyHideInGUITypes;          
        }

        public GUIType[] HideInGUITypes { get; set; } = Array.Empty<GUIType>();     
    }
}
