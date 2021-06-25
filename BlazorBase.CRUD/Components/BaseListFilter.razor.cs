using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Services;
using Blazorise;
using Blazorise.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Components.BaseDisplayComponent;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseListFilter
    {
        #region Parameters
        #region Events
        [Parameter] public EventCallback OnFilterChanged { get; set; }
        #endregion

        [Parameter] public Dictionary<string, DisplayGroup> DisplayGroups { get; set; } = new Dictionary<string, DisplayGroup>();
        #endregion

        #region Injects
        [Inject] protected IStringLocalizer<FilterType> FilterTypeLocalizer { get; set; }
        [Inject] protected BaseParser BaseParser { get; set; }
        #endregion

        #region Member  
        protected Dictionary<FilterType, KeyValuePair<string, string>> FilterTypes = new Dictionary<FilterType, KeyValuePair<string, string>>();
        protected List<KeyValuePair<string, string>> TextFilterTypes = new List<KeyValuePair<string, string>>();
        protected List<KeyValuePair<string, string>> NullableTextFilterTypes = new List<KeyValuePair<string, string>>();
        protected List<KeyValuePair<string, string>> NumberFilterTypes = new List<KeyValuePair<string, string>>();
        protected List<KeyValuePair<string, string>> NullableNumberFilterTypes = new List<KeyValuePair<string, string>>();
        protected List<KeyValuePair<string, string>> DateTimeFilterTypes = new List<KeyValuePair<string, string>>();
        protected List<KeyValuePair<string, string>> NullableDateTimeFilterTypes = new List<KeyValuePair<string, string>>();
        protected List<KeyValuePair<string, string>> BoolFilterTypes = new List<KeyValuePair<string, string>>();
        protected List<KeyValuePair<string, string>> NullableBoolFilterTypes = new List<KeyValuePair<string, string>>();

        public List<Type> AllowedFilterTypes = new List<Type>() {
            typeof(string),
            typeof(decimal),
            typeof(decimal?),
            typeof(double),
            typeof(double?),
            typeof(float),
            typeof(float?),
            typeof(int),
            typeof(int?),
            typeof(long),
            typeof(long?),
            typeof(bool),
            typeof(bool?),
            typeof(DateTime),
            typeof(DateTime?),
            typeof(Guid),
            typeof(Guid?)
        };
        protected List<DisplayItem> SortedColumns = new List<DisplayItem>();
        #endregion

        #region Init
        protected override void OnInitialized()
        {
            var filterTypes = Enum.GetValues(typeof(FilterType));
            foreach (FilterType filter in filterTypes)
                FilterTypes.Add(filter, new KeyValuePair<string, string>(filter.ToString(), FilterTypeLocalizer[filter.ToString()]));

            SetAllowedFilterTypes(NumberFilterTypes, FilterType.Like, FilterType.Equal, FilterType.Greater, FilterType.GreaterOrEqual, FilterType.Less, FilterType.LessOrEqual);
            SetAllowedFilterTypes(NullableNumberFilterTypes, FilterType.Like, FilterType.Equal, FilterType.Greater, FilterType.GreaterOrEqual, FilterType.Less, FilterType.LessOrEqual, FilterType.IsNull);
            SetAllowedFilterTypes(TextFilterTypes, FilterType.Like, FilterType.Equal, FilterType.IsEmpty);
            SetAllowedFilterTypes(NullableTextFilterTypes, FilterType.Like, FilterType.Equal, FilterType.IsEmpty, FilterType.IsNull);
            SetAllowedFilterTypes(BoolFilterTypes, FilterType.Equal);
            SetAllowedFilterTypes(NullableBoolFilterTypes, FilterType.Equal, FilterType.IsNull);
            SetAllowedFilterTypes(DateTimeFilterTypes, FilterType.Equal, FilterType.Greater, FilterType.GreaterOrEqual, FilterType.Less, FilterType.LessOrEqual);
            SetAllowedFilterTypes(NullableDateTimeFilterTypes, FilterType.Equal, FilterType.Greater, FilterType.GreaterOrEqual, FilterType.Less, FilterType.LessOrEqual, FilterType.IsNull);

            foreach (var displayGroup in DisplayGroups)
                foreach (var displayItem in displayGroup.Value.DisplayItems.Where(p => !p.IsListProperty))
                    if (displayItem.Property.PropertyType == typeof(bool) || displayItem.Property.PropertyType == typeof(bool?) || displayItem.Property.PropertyType == typeof(DateTime) || displayItem.Property.PropertyType == typeof(DateTime?))
                        displayItem.FilterType = FilterType.Equal;
        }

        protected virtual void SetAllowedFilterTypes(List<KeyValuePair<string, string>> list, params FilterType[] allowedFilterTypes)
        {
            foreach (var filter in allowedFilterTypes)
                list.Add(FilterTypes[filter]);
        }

        protected virtual DateInputMode GetDateInputMode(DisplayItem displayItem)
        {
            return displayItem.Property.GetCustomAttribute<DateDisplayModeAttribute>()?.DateInputMode ?? DateInputMode.Date;
        }
        #endregion

        #region Input Filtering
        protected async virtual Task FilterTypeChangedAsync(DisplayItem displayItem, string newFilterType)
        {
            if (!Enum.TryParse(typeof(FilterType), newFilterType, out object filterType))
                return;

            displayItem.FilterType = (FilterType)filterType;
            await OnFilterChanged.InvokeAsync();
        }

        protected async virtual Task FilterChangedAsync(DisplayItem displayItem, object newValue)
        {
            if (displayItem.Property.PropertyType != typeof(Guid) && displayItem.Property.PropertyType != typeof(Guid?))
                ConvertValueIfNeeded(ref newValue, displayItem.Property.PropertyType);
            displayItem.FilterValue = newValue;

            await OnFilterChanged.InvokeAsync();
        }

        protected void ConvertValueIfNeeded(ref object newValue, Type targetType)
        {
            if (newValue == null || newValue.GetType() == targetType)
                return;

            if (BaseParser.TryParseValueFromString(targetType, newValue.ToString(), out object parsedValue, out string errorMessage))
                newValue = parsedValue;
            else
                newValue = null;
        }
        #endregion
    }
}
