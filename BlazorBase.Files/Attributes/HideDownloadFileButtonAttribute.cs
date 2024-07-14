using BlazorBase.Abstractions.CRUD.Enums;
using System;

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
