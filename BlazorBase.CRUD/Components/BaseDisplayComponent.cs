using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Extensions;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components
{
    public class BaseDisplayComponent : ComponentBase
    {
        protected List<PropertyInfo> VisibleProperties = new List<PropertyInfo>();
        protected Dictionary<string, List<(VisibleAttribute Attribute, PropertyInfo Property)>> DisplayGroups = new Dictionary<string, List<(VisibleAttribute Attribute, PropertyInfo Property)>>();
        protected Dictionary<PropertyInfo, VisibleAttribute> ListProperties = new Dictionary<PropertyInfo, VisibleAttribute>();

        protected void SetUpDisplayLists(Type modelType, GUIType guiType)
        {
            VisibleProperties = modelType.GetVisibleProperties(guiType);

            foreach (var property in VisibleProperties)
            {
                var attribute = property.GetCustomAttributes(typeof(VisibleAttribute)).First() as VisibleAttribute;
                attribute.DisplayGroup = String.IsNullOrEmpty(attribute.DisplayGroup) ? "General" : attribute.DisplayGroup;

                if (property.IsListProperty())
                {
                    ListProperties.Add(property, attribute);
                    continue;
                }

                if (!DisplayGroups.ContainsKey(attribute.DisplayGroup))
                    DisplayGroups[attribute.DisplayGroup] = new List<(VisibleAttribute Attribute, PropertyInfo Property)>();

                DisplayGroups[attribute.DisplayGroup].Add((attribute, property));
            }

            SortDisplayLists();
        }

        protected void SortDisplayLists()
        {
            DisplayGroups = DisplayGroups.OrderBy(entry => entry.Value.FirstOrDefault().Attribute.DisplayGroupOrder).ToDictionary(x => x.Key, x => x.Value);

            foreach (var properties in DisplayGroups)
                properties.Value.Sort((x, y) => x.Attribute.DisplayOrder.CompareTo(y.Attribute.DisplayOrder));

            ListProperties = ListProperties.OrderBy(entry => entry.Value.DisplayGroupOrder).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
