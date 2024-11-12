using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Components.SelectList;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using Blazorise.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Components.SelectList.BaseTypeBasedSelectList;

namespace BlazorBase.CRUD.Components.Inputs;

public partial class BaseSelectListPopupInput : BaseInput
{
    #region Injects
    [Inject] protected IStringLocalizer<BaseSelectListInput> Localizer { get; set; } = null!;
    #endregion

    #region Member
    protected BaseTypeBasedSelectList? BaseSelectList = null;

    protected string? SelectedValue;
    protected string? DisplayValue;
    protected SelectList<KeyValuePair<string, string>, string>? SelectList = null;
    protected bool InputReferencesBaseModelWithMultiplePrimaryKeys = false;
    protected List<PropertyInfo> ForeignKeyProperties = [];
    protected bool ForeignKeyIsOnBaseModel = false;
    protected Type? ForeignKeyBaseModelType = null;
    protected PropertyInfo? ForeignKeyBaseModelProperty = null;
    protected List<PropertyInfo> DisplayKeyProperties = [];

    protected BaseInputDisplayOptionsAttribute? BaseInputDisplayOptions;
    #endregion

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var foreignKey = Property.GetCustomAttribute<ForeignKeyAttribute>();
        BaseInputDisplayOptions = Property.GetCustomAttribute<BaseInputDisplayOptionsAttribute>();
        if (foreignKey == null)
            throw new Exception($"Property {Property.Name} is not a foreign key property.");

        ForeignKeyIsOnBaseModel = typeof(IBaseModel).IsAssignableFrom(Property.PropertyType);
        if (ForeignKeyIsOnBaseModel && foreignKey!.Name.Contains(','))
        {
            ForeignKeyProperties = Property.PropertyType.GetKeyProperties();
            InputReferencesBaseModelWithMultiplePrimaryKeys = true;
        }

        if (ForeignKeyIsOnBaseModel)
            ForeignKeyBaseModelType = Property.PropertyType;
        else
        {
            ForeignKeyBaseModelProperty = Property.ReflectedType?.GetProperties().Where(entry => entry.Name == foreignKey!.Name).FirstOrDefault();
            ForeignKeyBaseModelType = ForeignKeyBaseModelProperty?.PropertyType;
        }

        if (ForeignKeyBaseModelType != null && ForeignKeyBaseModelType.IsInterface)
        {
            var type = ServiceProvider.GetService(ForeignKeyBaseModelType)?.GetType();
            if (type != null && typeof(IBaseModel).IsAssignableFrom(type))
                ForeignKeyBaseModelType = type;
        }

        ArgumentNullException.ThrowIfNull(ForeignKeyBaseModelType);
        DisplayKeyProperties = ForeignKeyBaseModelType.GetDisplayKeyProperties();

        UpdateSelectedValue();

        DisplayValue = GetDisplayText();
    }

    protected override void OnParametersSet()
    {
        DisplayValue = GetDisplayText();

        base.OnParametersSet();
    }

    protected void UpdateSelectedValue()
    {
        var value = Property.GetValue(Model);
        if (ForeignKeyIsOnBaseModel && value != null)
            SelectedValue = JsonConvert.SerializeObject(((IBaseModel)value).GetPrimaryKeys());
        else
            SelectedValue = JsonConvert.SerializeObject(new object?[] { value });
    }

    protected async virtual Task OnSelectedValueChangedAsync(string selectedValue)
    {
        if (selectedValue == null)
        {
            await OnValueChangedAsync(selectedValue);
            UpdateSelectedValue();
            return;
        }

        var primaryKeys = JsonConvert.DeserializeObject<object?[]>(selectedValue);
        if (primaryKeys == null)
            return;

        if (!InputReferencesBaseModelWithMultiplePrimaryKeys || !ForeignKeyIsOnBaseModel)
        {
            await OnValueChangedAsync(primaryKeys[0]);
            UpdateSelectedValue();
            return;
        }

        for (int i = 0; i < primaryKeys.Length; i++)
            primaryKeys[i] = Convert.ChangeType(primaryKeys[i], ForeignKeyProperties[i].PropertyType, CultureInfo.InvariantCulture);

        var entry = await DbContext.FindAsync(ForeignKeyBaseModelType!, primaryKeys);
        await OnValueChangedAsync(entry);
        UpdateSelectedValue();
    }

    protected override void Model_OnForcePropertyRepaint(object? sender, string[] propertyNames)
    {
        if (!propertyNames.Contains(Property.Name))
            return;

        UpdateSelectedValue();
        DisplayValue = GetDisplayText();

        base.Model_OnForcePropertyRepaint(sender, propertyNames);
    }

    protected virtual string GetDisplayText()
    {
        var property = Property;
        if (!ForeignKeyIsOnBaseModel)
        {
            if (ForeignKeyBaseModelProperty == null)
                throw new Exception("Cant get foreign key base model property. Make sure the visible attribute is on the base model property when multiple primary keys are defined.");

            property = ForeignKeyBaseModelProperty;
        }

        var value = property.GetValue(Model);
        if (value == null || value is not IBaseModel baseModel)
            return String.Empty;

        var primaryKeys = baseModel.GetPrimaryKeys();
        if (DisplayKeyProperties.Count == 0)
            return String.Join(", ", primaryKeys ?? Array.Empty<string>());
        else
            return baseModel.GetDisplayKeyKeyValuePair(DisplayKeyProperties) ?? String.Empty;
    }

    protected Task OpenForeignKeySelectListModalAsync(object? aboveEntry = null)
    {
        BaseSelectList?.ShowModal(aboveEntry);

        return Task.CompletedTask;
    }

    protected async Task AddEntryFromSelectListModalAsync(OnSelectListClosedArgs args)
    {
        if (args.SelectedModel == null)
            return;

        if (ForeignKeyIsOnBaseModel)
        {
            var entry = await DbContext.FindAsync(ForeignKeyBaseModelType!, args.SelectedModel.GetPrimaryKeys());
            await OnValueChangedAsync(entry);
        }
        else
        {
            var primaryKeys = args.SelectedModel.GetPrimaryKeys();
            await OnValueChangedAsync(primaryKeys?[0]);
        }

        UpdateSelectedValue();

        _ = InvokeAsync(StateHasChanged);
    }

    protected async Task ClearValueAsync()
    {
        await OnValueChangedAsync(null);
        UpdateSelectedValue();
    }
}