using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Components.General;
using BlazorBase.CRUD.Components.Inputs;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Helper;
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

namespace BlazorBase.CRUD.Components.List;

public partial class BaseListFilter : BaseDisplayComponent
{
    #region Parameters
    #region Events
    [Parameter] public EventCallback OnFilterChanged { get; set; }
    #endregion

    [Parameter] public Dictionary<string, DisplayGroup> ListDisplayGroups { get; set; } = new();

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public string? Class { get; set; }
    #endregion

    #region Injects
    [Inject] protected IStringLocalizer<BaseListFilter> BaseListFilterLocalizer { get; set; } = null!;
    [Inject] protected IStringLocalizer<FilterType> FilterTypeLocalizer { get; set; } = null!;
    [Inject] protected IStringLocalizer<BooleanValue> BooleanValueLocalizer { get; set; } = null!;
    [Inject] protected BaseParser BaseParser { get; set; } = null!;
    #endregion

    #region Member  
    protected Dictionary<FilterType, KeyValuePair<FilterType, string>> FilterTypes = new();
    protected Dictionary<Type, List<KeyValuePair<FilterType, string>>> TypeFilterTypesDictionary = new();
    protected List<KeyValuePair<FilterType, string>> TextFilterTypes = new();
    protected List<KeyValuePair<FilterType, string>> NullableTextFilterTypes = new();
    protected List<KeyValuePair<FilterType, string>> GuidFilterTypes = new();
    protected List<KeyValuePair<FilterType, string>> NullableGuidFilterTypes = new();
    protected List<KeyValuePair<FilterType, string>> NumberFilterTypes = new();
    protected List<KeyValuePair<FilterType, string>> NullableNumberFilterTypes = new();
    protected List<KeyValuePair<FilterType, string>> DateTimeFilterTypes = new();
    protected List<KeyValuePair<FilterType, string>> NullableDateTimeFilterTypes = new();
    protected List<KeyValuePair<FilterType, string>> BoolFilterTypes = new();
    protected List<KeyValuePair<FilterType, string>> NullableBoolFilterTypes = new();
    protected List<KeyValuePair<FilterType, string>> EnumFilterTypes = new();
    protected List<KeyValuePair<FilterType, string>> NullableEnumFilterTypes = new();

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
        typeof(TimeSpan),
        typeof(TimeSpan?),
        typeof(Guid),
        typeof(Guid?)
    };
    protected List<DisplayItem> SortedColumns = new();

    protected SelectList<KeyValuePair<string, string>, string>? AddToSelectLists { set { SelectLists.Add(value); } }
    protected List<SelectList<KeyValuePair<string, string>, string>?> SelectLists = new();
    protected TextEdit? AddToTextEdits { set { TextEdits.Add(value); } }
    protected List<TextEdit?> TextEdits = new();

    protected ComponentBase? AddToComponents { set { Components.Add(value); } }
    protected List<ComponentBase?> Components = new();

    #endregion

    #region Init
    protected override void OnInitialized()
    {
        var filterTypes = Enum.GetValues(typeof(FilterType));
        foreach (FilterType filter in filterTypes)
            FilterTypes.Add(filter, new KeyValuePair<FilterType, string>(filter, FilterTypeLocalizer[filter.ToString()]));

        SetAllowedFilterTypes(TextFilterTypes, FilterType.Like, FilterType.NotLike, FilterType.Equal, FilterType.NotEqual, FilterType.IsEmpty);
        SetAllowedFilterTypes(NullableTextFilterTypes, FilterType.Like, FilterType.NotLike, FilterType.Equal, FilterType.NotEqual, FilterType.IsEmpty, FilterType.NotEmpty, FilterType.IsNull, FilterType.NotNull);
        SetAllowedFilterTypes(GuidFilterTypes, FilterType.Like, FilterType.NotLike, FilterType.IsEmpty, FilterType.NotEmpty);
        SetAllowedFilterTypes(NullableGuidFilterTypes, FilterType.Like, FilterType.NotLike, FilterType.IsEmpty, FilterType.NotEmpty, FilterType.IsNull, FilterType.NotNull);
        SetAllowedFilterTypes(NumberFilterTypes, FilterType.Like, FilterType.NotLike, FilterType.Equal, FilterType.NotEqual, FilterType.Greater, FilterType.NotGreater, FilterType.GreaterOrEqual, FilterType.NotGreaterOrEqual, FilterType.Less, FilterType.NotLess, FilterType.LessOrEqual, FilterType.NotLessOrEqual);
        SetAllowedFilterTypes(NullableNumberFilterTypes, FilterType.Like, FilterType.NotLike, FilterType.Equal, FilterType.NotEqual, FilterType.Greater, FilterType.NotGreater, FilterType.GreaterOrEqual, FilterType.NotGreaterOrEqual, FilterType.Less, FilterType.NotLess, FilterType.LessOrEqual, FilterType.NotLessOrEqual, FilterType.IsNull, FilterType.NotNull);
        SetAllowedFilterTypes(BoolFilterTypes, FilterType.Equal, FilterType.NotEqual);
        SetAllowedFilterTypes(NullableBoolFilterTypes, FilterType.Equal, FilterType.NotEqual, FilterType.IsNull, FilterType.NotNull);
        SetAllowedFilterTypes(DateTimeFilterTypes, FilterType.Equal, FilterType.NotEqual, FilterType.Greater, FilterType.NotGreater, FilterType.GreaterOrEqual, FilterType.NotGreaterOrEqual, FilterType.Less, FilterType.NotLess, FilterType.LessOrEqual, FilterType.NotLessOrEqual);
        SetAllowedFilterTypes(NullableDateTimeFilterTypes, FilterType.Equal, FilterType.NotEqual, FilterType.Greater, FilterType.NotGreater, FilterType.GreaterOrEqual, FilterType.NotGreaterOrEqual, FilterType.Less, FilterType.NotLess, FilterType.LessOrEqual, FilterType.NotLessOrEqual, FilterType.IsNull, FilterType.NotNull);
        SetAllowedFilterTypes(EnumFilterTypes, FilterType.Equal, FilterType.NotEqual);
        SetAllowedFilterTypes(NullableEnumFilterTypes, FilterType.Equal, FilterType.NotEqual, FilterType.IsNull, FilterType.NotNull);
        FillTypeFilterTypesDictionary();
        CorrectInitialFilterTypes();

        BoolSelectListData.Add(new KeyValuePair<string, string>(BooleanValue.NotSet.ToString(), BooleanValueLocalizer[BooleanValue.NotSet.ToString()]));
        BoolSelectListData.Add(new KeyValuePair<string, string>(BooleanValue.True.ToString(), BooleanValueLocalizer[BooleanValue.True.ToString()]));
        BoolSelectListData.Add(new KeyValuePair<string, string>(BooleanValue.False.ToString(), BooleanValueLocalizer[BooleanValue.False.ToString()]));
    }

    protected virtual void SetAllowedFilterTypes(List<KeyValuePair<FilterType, string>> list, params FilterType[] allowedFilterTypes)
    {
        foreach (var filter in allowedFilterTypes)
            list.Add(FilterTypes[filter]);
    }

    protected virtual void FillTypeFilterTypesDictionary()
    {
        foreach (var displayGroup in ListDisplayGroups)
        {
            foreach (var displayItem in displayGroup.Value.DisplayItems.Where(p => !p.IsListProperty))
            {
                if (TypeFilterTypesDictionary.ContainsKey(displayItem.DisplayPropertyType))
                    continue;

                if (displayItem.DisplayPropertyType == typeof(string))
                    TypeFilterTypesDictionary[displayItem.DisplayPropertyType] = Nullable.GetUnderlyingType(displayItem.DisplayPropertyType) == null ? TextFilterTypes : NullableTextFilterTypes;
                else if (TypeHelper.NumericTypes.Contains(displayItem.DisplayPropertyType))
                    TypeFilterTypesDictionary[displayItem.DisplayPropertyType] = Nullable.GetUnderlyingType(displayItem.DisplayPropertyType) == null ? NumberFilterTypes : NullableNumberFilterTypes;
                else if (displayItem.DisplayPropertyType == typeof(bool))
                    TypeFilterTypesDictionary[displayItem.DisplayPropertyType] = BoolFilterTypes;
                else if (displayItem.DisplayPropertyType == typeof(bool?))
                    TypeFilterTypesDictionary[displayItem.DisplayPropertyType] = NullableBoolFilterTypes;
                else if (displayItem.DisplayPropertyType == typeof(DateTime) || displayItem.DisplayPropertyType == typeof(TimeSpan))
                    TypeFilterTypesDictionary[displayItem.DisplayPropertyType] = DateTimeFilterTypes;
                else if (displayItem.DisplayPropertyType == typeof(DateTime?) || displayItem.DisplayPropertyType == typeof(TimeSpan?))
                    TypeFilterTypesDictionary[displayItem.DisplayPropertyType] = NullableDateTimeFilterTypes;
                else if (displayItem.DisplayPropertyType == typeof(Guid))
                    TypeFilterTypesDictionary[displayItem.DisplayPropertyType] = GuidFilterTypes;
                else if (displayItem.DisplayPropertyType == typeof(Guid?))
                    TypeFilterTypesDictionary[displayItem.DisplayPropertyType] = NullableGuidFilterTypes;
                else if (displayItem.DisplayPropertyType.IsEnum)
                    TypeFilterTypesDictionary[displayItem.DisplayPropertyType] = Nullable.GetUnderlyingType(displayItem.DisplayPropertyType) == null ? EnumFilterTypes : NullableEnumFilterTypes;
            }
        }
    }

    protected virtual DateInputMode GetDateInputMode(DisplayItem displayItem)
    {
        return displayItem.Property.GetCustomAttribute<DateDisplayModeAttribute>()?.DateInputMode ?? DateInputMode.Date;
    }

    protected override List<KeyValuePair<string?, string>> GetEnumValues(Type enumType)
    {
        long key = GetEnumTypeDictionaryKey(enumType);

        if (CachedEnumValueDictionary.ContainsKey(key))
            return CachedEnumValueDictionary[key];

        var enumValues = base.GetEnumValues(enumType);
        enumValues.Insert(0, new KeyValuePair<string?, string>(null, String.Empty));
        return enumValues;
    }

    protected virtual void CorrectInitialFilterTypes()
    {
        foreach (var displayGroup in ListDisplayGroups)
            foreach (var displayItem in displayGroup.Value.DisplayItems.Where(p => !p.IsListProperty))
                if (TypeFilterTypesDictionary.ContainsKey(displayItem.DisplayPropertyType) && !TypeFilterTypesDictionary[displayItem.DisplayPropertyType].Any(entry => entry.Key == displayItem.FilterType))
                    displayItem.FilterType = TypeFilterTypesDictionary[displayItem.DisplayPropertyType].First().Key;
    }

    #endregion

    #region Input Filtering

    protected async virtual Task FilterTypeChangedAsync(DisplayItem displayItem, FilterType newFilterType)
    {
        displayItem.FilterType = newFilterType;
        await OnFilterChanged.InvokeAsync();
    }

    protected async virtual Task FilterChangedAsync(DisplayItem displayItem, object? newValue)
    {
        if (displayItem.DisplayPropertyType != typeof(Guid) && displayItem.DisplayPropertyType != typeof(Guid?))
            ConvertValueIfNeeded(ref newValue, displayItem.DisplayPropertyType);

        displayItem.FilterValue = newValue;

        await OnFilterChanged.InvokeAsync();
    }

    protected async virtual Task BooleanFilterChangedAsync(DisplayItem displayItem, string newValue)
    {
        if (!Enum.TryParse(typeof(BooleanValue), newValue, out object? filterType))
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
                if (TypeFilterTypesDictionary.ContainsKey(displayItem.DisplayPropertyType))
                {
                    displayItem.FilterType = TypeFilterTypesDictionary[displayItem.DisplayPropertyType].First().Key;
                    displayItem.FilterValue = null;
                }

        await OnFilterChanged.InvokeAsync();
    }

    protected virtual void ConvertValueIfNeeded(ref object? newValue, Type targetType)
    {
        if (newValue == null || newValue.GetType() == targetType)
            return;

        if (BaseParser.TryParseValueFromString(targetType, newValue.ToString(), out object? parsedValue, out _))
            newValue = parsedValue;
        else
            newValue = null;
    }
    #endregion
}
