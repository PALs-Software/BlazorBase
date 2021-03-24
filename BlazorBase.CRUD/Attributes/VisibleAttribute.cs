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
        public VisibleAttribute() { }

        public VisibleAttribute(string displaygroup, int displayGroupOrder = 0, int displayOrder = 0, bool collapsed = false)
        {
            DisplayGroup = displaygroup;
            DisplayGroupOrder = displayGroupOrder;
            DisplayOrder = displayOrder;
            Collapsed = collapsed;
        }

        public VisibleAttribute(params GUIType[] hideInGUITypes)
        {
            HideInGUITypes = hideInGUITypes.ToList();
        }

        public VisibleAttribute(string displaygroup, params GUIType[] hideInGUITypes)
        {
            HideInGUITypes = hideInGUITypes.ToList();
            DisplayGroup = displaygroup;
        }

        public VisibleAttribute(string displaygroup, int displayGroupOrder = 0, int displayOrder = 0, bool collapsed = false, params GUIType[] hideInGUITypes)
        {
            HideInGUITypes = hideInGUITypes.ToList();
            DisplayGroupOrder = displayGroupOrder;
            DisplayGroup = displaygroup;
            DisplayOrder = displayOrder;
            Collapsed = collapsed;
        }

        public List<GUIType> HideInGUITypes { get; set; } = new List<GUIType>();

        public string DisplayGroup { get; set; }

        public int DisplayGroupOrder { get; set; }

        public int DisplayOrder { get; set; }

        public bool Collapsed { get; set; }
    }
}
