using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Components.General;
using BlazorBase.CRUD.Components.Inputs;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Services;
using Blazorise;
using Blazorise.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.List
{
    public partial class BaseListFilter : BaseDisplayComponent
    {
        #region Parameters
        #region Events
        [Parameter] public EventCallback OnFilterChanged { get; set; }
        #endregion

        [Parameter] public Dictionary<string, DisplayGroup> ListDisplayGroups { get; set; } = new();

        [Parameter] public RenderFragment ChildContent { get; set; }
        #endregion

        #region Injects
        [Inject] protected IStringLocalizer<BaseListFilter> BaseListFilterLocalizer { get; set; }
        [Inject] protected IStringLocalizer<FilterType> FilterTypeLocalizer { get; set; }
        [Inject] protected IStringLocalizer<BooleanValue> BooleanValueLocalizer { get; set; }
        [Inject] protected BaseParser BaseParser { get; set; }
        #endregion

        #region Member  
        protected Dictionary<FilterType, KeyValuePair<string, string>> FilterTypes = new();
        protected List<KeyValuePair<string, string>> TextFilterTypes = new();
        protected List<KeyValuePair<string, string>> NullableTextFilterTypes = new();
        protected List<KeyValuePair<string, string>> GuidFilterTypes = new();
        protected List<KeyValuePair<string, string>> NullableGuidFilterTypes = new();
        protected List<KeyValuePair<string, string>> NumberFilterTypes = new();
        protected List<KeyValuePair<string, string>> NullableNumberFilterTypes = new();
        protected List<KeyValuePair<string, string>> DateTimeFilterTypes = new();
        protected List<KeyValuePair<string, string>> NullableDateTimeFilterTypes = new();
        protected List<KeyValuePair<string, string>> BoolFilterTypes = new();
        protected List<KeyValuePair<string, string>> NullableBoolFilterTypes = new();
        protected List<KeyValuePair<string, string>> EnumFilterTypes = new();
        protected List<KeyValuePair<string, string>> NullableEnumFilterTypes = new();

        protected List<KeyValuePair<string, string>> BoolSelectListData = new();

        public List<Type> AllowedFilterTypes = new()
        {
            typeof(string),
            typeof(decimal),
            typeof(decimal?),
            typeof(double),
            typeof(double?),
            typeof(float),
            typeof(float?),
            typeof(uint),
            typeof(uint?),
            typeof(int),
            typeof(int?),
            typeof(long),
            typeof(long?),
            typeof(ulong),
            typeof(ulong?),
            typeof(bool),
            typeof(bool?),
            typeof(DateTime),
            typeof(DateTime?),
            typeof(Guid),
            typeof(Guid?)
        };
        protected List<DisplayItem> SortedColumns = new();

        protected SelectList<KeyValuePair<string, string>, string> AddToSelectLists { set { SelectLists.Add(value); } }
        protected List<SelectList<KeyValuePair<string, string>, string>> SelectLists = new();
        protected TextEdit AddToTextEdits { set { TextEdits.Add(value); } }
        protected List<TextEdit> TextEdits = new();

        protected ComponentBase AddToComponents { set { Components.Add(value); } }
        protected List<ComponentBase> Components = new();

        #endregion

        #region Init
        protected override void OnInitialized()
        {
            var filterTypes = Enum.GetValues(typeof(FilterType));
            foreach (FilterType filter in filterTypes)
                FilterTypes.Add(filter, new KeyValuePair<string, string>(filter.ToString(), FilterTypeLocalizer[filter.ToString()]));

            SetAllowedFilterTypes(TextFilterTypes, FilterType.Like, FilterType.Equal, FilterType.IsEmpty);
            SetAllowedFilterTypes(NullableTextFilterTypes, FilterType.Like, FilterType.Equal, FilterType.IsEmpty, FilterType.IsNull);
            SetAllowedFilterTypes(GuidFilterTypes, FilterType.Like, FilterType.IsEmpty);
            SetAllowedFilterTypes(NullableGuidFilterTypes, FilterType.Like, FilterType.IsEmpty, FilterType.IsNull);
            SetAllowedFilterTypes(NumberFilterTypes, FilterType.Like, FilterType.Equal, FilterType.Greater, FilterType.GreaterOrEqual, FilterType.Less, FilterType.LessOrEqual);
            SetAllowedFilterTypes(NullableNumberFilterTypes, FilterType.Like, FilterType.Equal, FilterType.Greater, FilterType.GreaterOrEqual, FilterType.Less, FilterType.LessOrEqual, FilterType.IsNull);
            SetAllowedFilterTypes(BoolFilterTypes, FilterType.Equal);
            SetAllowedFilterTypes(NullableBoolFilterTypes, FilterType.Equal, FilterType.IsNull);
            SetAllowedFilterTypes(DateTimeFilterTypes, FilterType.Equal, FilterType.Greater, FilterType.GreaterOrEqual, FilterType.Less, FilterType.LessOrEqual);
            SetAllowedFilterTypes(NullableDateTimeFilterTypes, FilterType.Equal, FilterType.Greater, FilterType.GreaterOrEqual, FilterType.Less, FilterType.LessOrEqual, FilterType.IsNull);
            SetAllowedFilterTypes(EnumFilterTypes, FilterType.Equal);
            SetAllowedFilterTypes(NullableEnumFilterTypes, FilterType.Equal, FilterType.IsNull);

            BoolSelectListData.Add(new KeyValuePair<string, string>(BooleanValue.NotSet.ToString(), BooleanValueLocalizer[BooleanValue.NotSet.ToString()]));
            BoolSelectListData.Add(new KeyValuePair<string, string>(BooleanValue.True.ToString(), BooleanValueLocalizer[BooleanValue.True.ToString()]));
            BoolSelectListData.Add(new KeyValuePair<string, string>(BooleanValue.False.ToString(), BooleanValueLocalizer[BooleanValue.False.ToString()]));

            foreach (var displayGroup in ListDisplayGroups)
                foreach (var displayItem in displayGroup.Value.DisplayItems.Where(p => !p.IsListProperty))
                    if (displayItem.DisplayPropertyType == typeof(bool) || displayItem.DisplayPropertyType == typeof(bool?) ||
                        displayItem.DisplayPropertyType == typeof(DateTime) || displayItem.DisplayPropertyType == typeof(DateTime?) ||
                        displayItem.DisplayPropertyType.IsEnum
                        )
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

        protected override List<KeyValuePair<string, string>> GetEnumValues(Type enumType)
        {
            long key = GetEnumTypeDictionaryKey(enumType);

            if (CachedEnumValueDictionary.ContainsKey(key))
                return CachedEnumValueDictionary[key];

            var enumValues = base.GetEnumValues(enumType);
            enumValues.Insert(0, new KeyValuePair<string, string>(null, String.Empty));
            return enumValues;
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
            if (displayItem.DisplayPropertyType != typeof(Guid) && displayItem.DisplayPropertyType != typeof(Guid?))
                ConvertValueIfNeeded(ref newValue, displayItem.DisplayPropertyType);

            displayItem.FilterValue = newValue;

            await OnFilterChanged.InvokeAsync();
        }

        protected async virtual Task BooleanFilterChangedAsync(DisplayItem displayItem, string newValue)
        {
            if (!Enum.TryParse(typeof(BooleanValue), newValue, out object filterType))
                return;

            switch ((BooleanValue)filterType)
            {
                case BooleanValue.Null:
                case BooleanValue.NotSet:
                    displayItem.FilterValue = null;
                    break;
                case BooleanValue.True:
                    displayItem.FilterValue = true;
                    break;
                case BooleanValue.False:
                    displayItem.FilterValue = false;
                    break;
            }

            await OnFilterChanged.InvokeAsync();
        }

        protected async virtual Task ResetAllFiltersAsync()
        {
            foreach (var displayGroup in ListDisplayGroups)
                foreach (var displayItem in displayGroup.Value.DisplayItems.Where(p => !p.IsListProperty))
                {
                    if (displayItem.DisplayPropertyType == typeof(bool) || displayItem.DisplayPropertyType == typeof(bool?) || displayItem.DisplayPropertyType == typeof(DateTime) || displayItem.DisplayPropertyType == typeof(DateTime?))
                        displayItem.FilterType = FilterType.Equal;
                    else
                        displayItem.FilterType = FilterType.Like;

                    displayItem.FilterValue = null;
                }
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
            foreach (var component in Components)
                if (component is SelectList<KeyValuePair<string, string>, string> selectList)
                    selectList.SelectedValue = selectList.Data.First().Key;
                else if (component is TextEdit textEdit)
                    textEdit.Text = String.Empty;
                else if (component is DateEdit<DateTime?> dateEdit)
                    dateEdit.Date = null;
                else if (component is BaseNumberFilterInput numberFilterInput)
                    numberFilterInput.Value = null;
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.

            await OnFilterChanged.InvokeAsync();
        }

        protected virtual void ConvertValueIfNeeded(ref object newValue, Type targetType)
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
