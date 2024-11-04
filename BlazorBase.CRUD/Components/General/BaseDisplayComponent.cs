﻿using BlazorBase.Abstractions.CRUD.Arguments;
using BlazorBase.Abstractions.CRUD.Attributes;
using BlazorBase.Abstractions.CRUD.Enums;
using BlazorBase.Abstractions.CRUD.Extensions;
using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.Abstractions.CRUD.Structures;
using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.Modules;
using BlazorBase.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.General;

public class BaseDisplayComponent : ComponentBase
{
    #region Parameter
    [Parameter] public EventCallback<OnAfterGetVisiblePropertiesArgs> OnAfterGetVisibleProperties { get; set; }
    [Parameter] public EventCallback<OnAfterSetUpDisplayListsArgs> OnAfterSetUpDisplayLists { get; set; }
    #endregion

    #region Injects        
    [Inject] protected IServiceProvider ServiceProvider { get; set; } = null!;
    [Inject] protected BaseErrorHandler ErrorHandler { get; set; } = null!;
    [Inject] protected IStringLocalizerFactory StringLocalizerFactory { get; set; } = null!;
    [Inject] protected IStringLocalizer<BaseDisplayComponent> BaseDisplayComponentLocalizer { get; set; } = null!;
    [Inject] protected IBaseAuthenticationService BaseAuthenticationService { get; set; } = null!;
    #endregion

    #region Protected Properties
    protected virtual Dictionary<PropertyInfo, IDisplayItem> VisibleProperties { get; set; } = [];
    protected virtual Dictionary<string, IDisplayGroup> DisplayGroups { get; set; } = [];
    protected virtual Dictionary<PropertyInfo, List<KeyValuePair<string?, string>>> ForeignKeyProperties { get; set; } = null!;
    protected static ConcurrentDictionary<long, List<KeyValuePair<string?, string>>> CachedEnumValueDictionary { get; set; } = new();
    protected virtual Dictionary<Type, List<KeyValuePair<string?, string>>> CachedForeignKeys { get; set; } = [];
    protected virtual Dictionary<PropertyInfo, (MethodInfo Method, List<KeyValuePair<string?, string>> LookupData)> UsesCustomLookupDataProperties { get; set; } = [];
    #endregion

    #region Member
    protected MarkupString InvalidSummaryFeedback;
    protected bool ShowInvalidFeedback = false;
    #endregion

    protected virtual async Task<List<PropertyInfo>> GetVisiblePropertiesAsync(Type modelType, GUIType guiType, List<string> userRoles, IBaseModel? componentModelInstance = null)
    {
        List<PropertyInfo> visibleProperties;
        if (componentModelInstance == null)
            visibleProperties = modelType.GetVisibleProperties(guiType, userRoles);
        else
            visibleProperties = componentModelInstance.GetVisibleProperties(guiType, userRoles);

        var args = new OnAfterGetVisiblePropertiesArgs(modelType, guiType, componentModelInstance, visibleProperties, userRoles);
        await OnAfterGetVisibleProperties.InvokeAsync(args);

        return args.VisibleProperties;
    }

    protected virtual async Task SetUpDisplayListsAsync(Type modelType, GUIType guiType, IBaseModel? componentModelInstance = null)
    {
        var userRoles = await BaseAuthenticationService.GetUserRolesAsync();
        VisibleProperties = [];
        DisplayGroups = [];
        var visibleProperties = await GetVisiblePropertiesAsync(modelType, guiType, userRoles, componentModelInstance);

        foreach (var property in visibleProperties)
        {
            var displayItem = DisplayItem.CreateFromProperty(property, guiType, ServiceProvider, userRoles, BaseDisplayComponentLocalizer["General"]);
            VisibleProperties.Add(property, displayItem);

            if (!DisplayGroups.ContainsKey(displayItem.Attribute.DisplayGroup ?? String.Empty))
                DisplayGroups[displayItem.Attribute.DisplayGroup ?? String.Empty] = new DisplayGroup(displayItem.Attribute, []);

            DisplayGroups[displayItem.Attribute.DisplayGroup ?? String.Empty].DisplayItems.Add(displayItem);
        }

        SortDisplayLists();

        var args = new OnAfterSetUpDisplayListsArgs(modelType, guiType, componentModelInstance, VisibleProperties, DisplayGroups, userRoles);
        await OnAfterSetUpDisplayLists.InvokeAsync(args);
    }

    protected virtual void SortDisplayLists()
    {
        foreach (var displayGroup in DisplayGroups)
        {
            displayGroup.Value.DisplayItems.Sort((x, y) => x.Attribute.DisplayOrder.CompareTo(y.Attribute.DisplayOrder));
            displayGroup.Value.GroupAttribute = displayGroup.Value.DisplayItems.First().Attribute;
        }

        DisplayGroups = DisplayGroups.OrderBy(entry => entry.Value.GroupAttribute.DisplayGroupOrder).ToDictionary(x => x.Key, x => x.Value);
    }

    protected virtual async Task PrepareForeignKeyPropertiesAsync(IBaseDbContext dbContext, IBaseModel? instance = null)
    {
        if (ForeignKeyProperties != null)
            return;

        ForeignKeyProperties = new Dictionary<PropertyInfo, List<KeyValuePair<string?, string>>>();

        var foreignKeyProperties = VisibleProperties.Where(entry => entry.Key.IsForeignKey());
        foreach (var foreignKeyPropertyPair in foreignKeyProperties)
        {
            var foreignKeyProperty = foreignKeyPropertyPair.Key;
            var foreignKey = foreignKeyProperty.GetCustomAttribute(typeof(ForeignKeyAttribute)) as ForeignKeyAttribute;
            if (foreignKey == null)
                continue;

            PropertyInfo? foreignProperty;
            if (foreignKey.Name.Contains(',')) // Reference has more than one primary key
                foreignProperty = foreignKeyProperty;
            else
                foreignProperty = foreignKeyProperty.ReflectedType?.GetProperties().Where(entry => entry.Name == foreignKey.Name).FirstOrDefault();
            var foreignKeyType = foreignProperty?.GetCustomAttribute<RenderTypeAttribute>()?.RenderType ?? foreignProperty?.PropertyType;

            if (foreignKeyType == null)
                throw new CRUDException(BaseDisplayComponentLocalizer["Can not find the foreign key property type in the class {0}, on the property {1}. This is a development error, maybe the foreign property name is spelled  wrong in the property attribute.", foreignKeyProperty.DeclaringType!, foreignKeyProperty.Name]);

            if (!typeof(IBaseModel).IsAssignableFrom(foreignKeyType))
                continue;

            if (foreignKeyType.IsInterface)
            {
                var type = ServiceProvider.GetService(foreignKeyType)?.GetType();
                if (type != null && typeof(IBaseModel).IsAssignableFrom(type))
                    foreignKeyType = type;
            }

            if (CachedForeignKeys.ContainsKey(foreignKeyType))
            {
                ForeignKeyProperties.Add(foreignKeyProperty, CachedForeignKeys[foreignKeyType]);
                continue;
            }

            var displayKeyProperties = foreignKeyType.GetDisplayKeyProperties();
            var primaryKeys = new List<KeyValuePair<string?, string>>()
            {
                new KeyValuePair<string?, string>(null, String.Empty)
            };

            if (foreignKeyPropertyPair.Value.IsReadOnly && instance != null)
            {
                var foreignKeyValue = foreignKeyProperty.GetValue(instance);
                if (foreignKeyValue != null)
                {
                    var entry = await dbContext.FindAsync(foreignKeyType, foreignKeyValue);
                    if (entry != null)
                        AddEntryToForeignKeyList((IBaseModel)entry, primaryKeys, displayKeyProperties);
                }

                ForeignKeyProperties.Add(foreignKeyProperty, primaryKeys);
                continue;
            }

            var entries = await dbContext.SetAsync(foreignKeyType, query =>
            {
                dynamic dynamicQuery = query;
                dynamicQuery = EntityFrameworkQueryableExtensions.AsNoTracking(dynamicQuery);
                for (int i = 0; i < displayKeyProperties.Count; i++)
                    dynamicQuery = i == 0 ? IQueryableExtension.OrderBy(dynamicQuery, displayKeyProperties[i].Name) : IQueryableExtension.ThenBy(dynamicQuery, displayKeyProperties[i].Name);

                return query.ToList();
            });

            foreach (var entry in entries)
                AddEntryToForeignKeyList((IBaseModel)entry, primaryKeys, displayKeyProperties);

            CachedForeignKeys.Add(foreignKeyType, primaryKeys);
            ForeignKeyProperties.Add(foreignKeyProperty, primaryKeys);
        }
    }

    protected void AddEntryToForeignKeyList(IBaseModel model, List<KeyValuePair<string?, string>> foreignKeyList, List<PropertyInfo> displayKeyProperties)
    {
        var primaryKeys = model.GetPrimaryKeys();
        var primaryKeysAsJson = JsonConvert.SerializeObject(primaryKeys);

        if (displayKeyProperties.Count == 0)
            foreignKeyList.Add(new KeyValuePair<string?, string>(primaryKeysAsJson, String.Join(", ", primaryKeys ?? Array.Empty<string>())));
        else
            foreignKeyList.Add(new KeyValuePair<string?, string>(primaryKeysAsJson, model?.GetDisplayKeyKeyValuePair(displayKeyProperties) ?? String.Empty));
    }

    protected virtual async Task PrepareCustomLookupData(IBaseModel model, EventServices eventServices)
    {
        UsesCustomLookupDataProperties.Clear();

        var properties = VisibleProperties.Keys.Where(entry => entry.UsesCustomLookupData());
        foreach (var property in properties)
        {
            var useCustomLookupData = property.GetCustomAttribute<UseCustomLookupDataAttribute>();
            var lookupDataSourceMethod = property.ReflectedType?.GetMethod(useCustomLookupData!.LookupDataSourceMethodName, BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
            var parameters = lookupDataSourceMethod?.GetParameters();

            if (lookupDataSourceMethod == null ||
                parameters == null ||
                parameters.Length != 4 ||
                parameters[0].ParameterType != typeof(PropertyInfo) ||
                parameters[1].ParameterType != typeof(IBaseModel) ||
                parameters[2].ParameterType != typeof(List<KeyValuePair<string?, string>>) ||
                parameters[3].ParameterType != typeof(EventServices) ||
                lookupDataSourceMethod.ReturnType != typeof(Task) ||
                !lookupDataSourceMethod.IsStatic)
                throw new CRUDException(BaseDisplayComponentLocalizer["The signature of the custom lookup data source method {0} in the class {1}, does not match the following signature: public static [async] Task TheMethodName(PropertyInfo propertyInfo, IBaseModel cardModel, List<KeyValuePair<string?, string>> lookupData, EventServices eventServices)", useCustomLookupData!.LookupDataSourceMethodName, property.ReflectedType?.Name ?? String.Empty]);

            var lookupData = new List<KeyValuePair<string?, string>>();
            var task = lookupDataSourceMethod.Invoke(null, new object[] { property, model, lookupData, eventServices });
            if (task != null)
                await (Task)task;

            UsesCustomLookupDataProperties.Add(property, (lookupDataSourceMethod, lookupData));
        }
    }

    protected virtual async Task RecalculateCustomLookupData(IBaseModel model, EventServices eventServices, params string[] propertyNames)
    {
        var lookupDataToRecalculate = propertyNames.Length == 0 ? UsesCustomLookupDataProperties : UsesCustomLookupDataProperties.Where(entry => propertyNames.Contains(entry.Key.Name));
        foreach (var lookupDataEntry in lookupDataToRecalculate)
        {
            var property = lookupDataEntry.Key;
            var lookupDataSourceMethod = lookupDataEntry.Value.Method;
            var lookupData = new List<KeyValuePair<string?, string>>();
            var task = lookupDataSourceMethod.Invoke(null, new object[] { property, model, lookupData, eventServices });
            if (task != null)
                await (Task)task;

            UsesCustomLookupDataProperties[property] = (lookupDataSourceMethod, lookupData);
        }
    }

    protected virtual long GetEnumTypeDictionaryKey(Type enumType)
    {
        return enumType.GetHashCode() * 10000L + CultureInfo.CurrentUICulture.LCID;
    }


    protected virtual List<KeyValuePair<string?, string>> GetEnumValues(Type enumType)
    {
        long key = GetEnumTypeDictionaryKey(enumType);
        if (CachedEnumValueDictionary.ContainsKey(key))
            return CachedEnumValueDictionary[key];

        var underlyingType = Nullable.GetUnderlyingType(enumType);
        if (underlyingType != null)
            enumType = underlyingType;

        var result = new List<KeyValuePair<string?, string>>();
        var values = Enum.GetNames(enumType);
        var localizer = StringLocalizerFactory.Create(enumType);
        foreach (var value in values)
            result.Add(new KeyValuePair<string?, string>(value, localizer[value]));

        CachedEnumValueDictionary.TryAdd(key, result);
        return result;
    }

    protected virtual Dictionary<string, string?> RemoveNavigationQueryByType(Type type, string baseQuery)
    {
        var query = QueryHelpers.ParseQuery(baseQuery).ToDictionary(key => key.Key, val => (string?)val.Value.ToString());

        var keyProperties = type.GetKeyProperties();

        var primaryKeys = new List<object>();
        foreach (var keyProperty in keyProperties)
            query.Remove(keyProperty.Name);

        return query;
    }

    #region Feedback

    protected virtual void ShowFormattedInvalidFeedback(string feedback)
    {
        InvalidSummaryFeedback = BaseMarkupStringValidator.GetWhiteListedMarkupString(feedback);
        ShowInvalidFeedback = true;
    }

    protected virtual void ResetInvalidFeedback()
    {
        InvalidSummaryFeedback = (MarkupString)String.Empty;
        ShowInvalidFeedback = false;
    }

    protected virtual async Task OnPageActionInvokedAsync(Exception e)
    {
        ResetInvalidFeedback();
        if (e != null)
            ShowFormattedInvalidFeedback(ErrorHandler.PrepareExceptionErrorMessage(e));

        await InvokeAsync(StateHasChanged);
    }
    #endregion

    public class DisplayGroup : IDisplayGroup
    {
        public DisplayGroup(VisibleAttribute groupAttribute, List<IDisplayItem> displayItems)
        {
            GroupAttribute = groupAttribute;
            DisplayItems = displayItems;
        }
        public VisibleAttribute GroupAttribute { get; set; }
        public List<IDisplayItem> DisplayItems { get; set; }
    }

    public class DisplayItem : IDisplayItem
    {
        public DisplayItem(PropertyInfo property, VisibleAttribute attribute, GUIType guiType, bool isReadonly, bool isKey, bool isListProperty, PresentationDataType? presentationDataType,
        string displayPropertyPath, Type displayPropertyType, bool isSortable, SortDirection sortDirection, bool isFilterable)
        {
            Property = property;
            Attribute = attribute;
            GUIType = guiType;
            IsReadOnly = isReadonly;
            IsKey = isKey;
            IsListProperty = isListProperty;
            PresentationDataType = presentationDataType;
            DisplayPropertyPath = displayPropertyPath;
            DisplayPropertyType = displayPropertyType;
            IsSortable = isSortable;
            SortDirection = sortDirection;
            IsFilterable = isFilterable;

            if (IsListProperty)
                IsPrimitiveListType = Property.PropertyType.GenericTypeArguments[0].IsPrimitive || Property.PropertyType.GenericTypeArguments[0] == typeof(string) || Property.PropertyType.GenericTypeArguments[0] == typeof(decimal);

            CustomizationAttributes = property.GetCustomAttributes<BaseCustomizationAttribute>().ToList();
            FillCustomizationAttributesClasses(guiType);
            FillCustomizationAttributesStyles(guiType);
        }

        #region Customization Attributes

        protected List<BaseCustomizationAttribute> CustomizationAttributes { get; set; } = [];
        public Dictionary<CustomizationLocation, string> CustomizationClasses { get; protected set; } = [];
        public Dictionary<CustomizationLocation, string> CustomizationStyles { get; protected set; } = [];

        protected void FillCustomizationAttributesClasses(GUIType guiType)
        {
            foreach (var location in Enum.GetValues<CustomizationLocation>())
            {
                CustomizationClasses[location] = String.Empty;

                foreach (var attribute in CustomizationAttributes)
                    if (attribute.ValidInGUITypes.Contains(guiType))
                        CustomizationClasses[location] += " " + attribute.GetClass(guiType, location);
            }
        }

        protected void FillCustomizationAttributesStyles(GUIType guiType)
        {
            foreach (var location in Enum.GetValues<CustomizationLocation>())
            {
                CustomizationStyles[location] = String.Empty;

                foreach (var attribute in CustomizationAttributes)
                    if (attribute.ValidInGUITypes.Contains(guiType))
                        CustomizationStyles[location] += " " + attribute.GetStyle(guiType, location);
            }
        }

        #endregion

        public PropertyInfo Property { get; set; }
        public VisibleAttribute Attribute { get; set; }
        public GUIType GUIType { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsKey { get; set; }
        public bool IsListProperty { get; set; }
        public bool IsPrimitiveListType { get; set; }
        public PresentationDataType? PresentationDataType { get; set; }
        public SortDirection SortDirection { get; set; }
        public FilterType FilterType { get; set; }
        public object? FilterValue { get; set; }
        public string? DisplayPropertyPath { get; set; }
        public Type DisplayPropertyType { get; set; }
        public bool IsSortable { get; set; }
        public bool IsFilterable { get; set; }

        public static IDisplayItem CreateFromProperty<T>(string propertyName, GUIType guiType, IServiceProvider serviceProvider, List<string>? userRoles = null, string? defaultDisplayGroup = null)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
                throw new Exception($"The property with the given property name \"{propertyName}\" does not exists");
            return CreateFromProperty(property, guiType, serviceProvider, userRoles, defaultDisplayGroup);
        }

        public static IDisplayItem CreateFromProperty(PropertyInfo property, GUIType guiType, IServiceProvider serviceProvider, List<string>? userRoles = null, string? defaultDisplayGroup = null)
        {
            var attribute = property.GetCustomAttributes(typeof(VisibleAttribute)).FirstOrDefault() as VisibleAttribute;
            if (attribute == null)
                throw new Exception($"The property \"{property.Name}\" has no visible attribut!\"");

            attribute.DisplayGroup = String.IsNullOrEmpty(attribute.DisplayGroup) ? defaultDisplayGroup : attribute.DisplayGroup;
            var presentationDataType = property.GetCustomAttribute<PresentationDataTypeAttribute>()?.Type;
            var customPropertyPath = property.GetCustomAttribute<CustomSortAndFilterPropertyPathAttribute>();
            var defaultListFilter = property.GetCustomAttribute<DefaultListFilterAttribute>();

            if (property.IsForeignKey() && typeof(IBaseModel).IsAssignableFrom(property.PropertyType))
            {
                var foreignKey = property.GetCustomAttribute<ForeignKeyAttribute>();
                if (foreignKey != null && foreignKey.Name.Contains(",")) // Reference has more than one primary key
                {
                    return new DisplayItem(property, attribute, guiType, property.IsReadOnlyInGUI(guiType, userRoles),
                            property.IsKey(), property.IsListProperty(), presentationDataType,
                            property.Name, property.PropertyType,
                            false, attribute.SortDirection, false)
                    { FilterType = defaultListFilter?.Type ?? FilterType.Like, FilterValue = defaultListFilter?.Value };
                }
            }

            (string DisplayPath, Type DisplayType) displayPathAndType;
            bool sortAndFilterable;
            if (customPropertyPath == null)
            {
                displayPathAndType = property.GetDisplayPropertyPathAndType(serviceProvider);
                sortAndFilterable = property.GetPropertyIsSortAndFilterable();
            }
            else
            {
                displayPathAndType = (customPropertyPath.Path, customPropertyPath.PathType);
                sortAndFilterable = true;
            }

            return new DisplayItem(property, attribute, guiType, property.IsReadOnlyInGUI(guiType, userRoles),
                property.IsKey(), property.IsListProperty(), presentationDataType,
                displayPathAndType.DisplayPath, displayPathAndType.DisplayType,
                sortAndFilterable, attribute.SortDirection, sortAndFilterable)
            { FilterType = defaultListFilter?.Type ?? FilterType.Like, FilterValue = defaultListFilter?.Value };
        }
    }
}
