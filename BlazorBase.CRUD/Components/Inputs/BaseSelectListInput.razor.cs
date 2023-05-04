using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using BlazorBase.CRUD.Models;
using System.Threading.Tasks;
using Blazorise.Components;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System;
using BlazorBase.CRUD.Extensions;
using System.Globalization;
using System.Linq;
using BlazorBase.CRUD.Components.SelectList;
using Microsoft.Extensions.Localization;
using static BlazorBase.CRUD.Components.SelectList.BaseTypeBasedSelectList;
using BlazorBase.CRUD.Attributes;

namespace BlazorBase.CRUD.Components.Inputs;

public partial class BaseSelectListInput : BaseInput
{
    #region Parameter
    [Parameter] public List<KeyValuePair<string, string>> Data { get; set; } = new();
    #endregion

    #region Injects
    [Inject] protected IStringLocalizer<BaseSelectListInput> Localizer { get; set; } = null!;
    #endregion

    #region Member
    protected BaseTypeBasedSelectList? BaseSelectList = null;

    protected string? SelectedValue;
    protected string? ReadOnlyDisplayValue;
    protected SelectList<KeyValuePair<string, string>, string>? SelectList = null;
    protected bool IsForeignKeyProperty = false;
    protected bool InputReferencesBaseModelWithMultiplePrimaryKeys = false;
    protected List<PropertyInfo> ForeignKeyProperties = new();
    protected bool ForeignKeyIsOnBaseModel = false;
    protected Type? ForeignKeyBaseModelType = null;

    protected BaseInputDisplayOptionsAttribute? BaseInputDisplayOptions;
    #endregion

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var foreignKey = Property.GetCustomAttribute<ForeignKeyAttribute>();
        BaseInputDisplayOptions = Property.GetCustomAttribute<BaseInputDisplayOptionsAttribute>();
        IsForeignKeyProperty = foreignKey != null;

        if (IsForeignKeyProperty)
        {
            ForeignKeyIsOnBaseModel = typeof(IBaseModel).IsAssignableFrom(Property.PropertyType);

            if (ForeignKeyIsOnBaseModel && foreignKey!.Name.Contains(','))
            {
                ForeignKeyProperties = Property.PropertyType.GetKeyProperties();
                InputReferencesBaseModelWithMultiplePrimaryKeys = true;
            }

            if (ForeignKeyIsOnBaseModel)
                ForeignKeyBaseModelType = Property.PropertyType;
            else
                ForeignKeyBaseModelType = Property.ReflectedType?.GetProperties().Where(entry => entry.Name == foreignKey!.Name).FirstOrDefault()?.PropertyType;
        }

        UpdateSelectedValue();

        if (IsReadOnly)
            ReadOnlyDisplayValue = GetReadOnlyDisplayText();
    }

    protected override void OnParametersSet()
    {
        if (IsReadOnly)
            ReadOnlyDisplayValue = GetReadOnlyDisplayText();

        base.OnParametersSet();
    }

    protected void UpdateSelectedValue()
    {
        var value = Property.GetValue(Model);
        if (ForeignKeyIsOnBaseModel && value != null)
            SelectedValue = JsonConvert.SerializeObject(((IBaseModel)value).GetPrimaryKeys());
        else if (IsForeignKeyProperty)
            SelectedValue = JsonConvert.SerializeObject(new object?[] { value });
        else
            SelectedValue = value?.ToString();
    }

    protected async virtual Task OnSelectedValueChangedAsync(string selectedValue)
    {
        if (!IsForeignKeyProperty || selectedValue == null)
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

        var entry = await Service.GetAsync(ForeignKeyBaseModelType!, primaryKeys);
        await OnValueChangedAsync(entry);
        UpdateSelectedValue();
    }

    protected virtual string GetReadOnlyDisplayText()
    {
        var foreignKeyPair = Data.FirstOrDefault(entry => entry.Key == SelectedValue);

        if (foreignKeyPair.Equals(default(KeyValuePair<string, string>)))
            return SelectedValue ?? String.Empty;
        else
            return foreignKeyPair.Value?.ToString() ?? String.Empty;
    }

    protected Task OpenForeignKeySelectListModal(object? aboveEntry = null)
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
            var entry = await Service.GetAsync(ForeignKeyBaseModelType!, args.SelectedModel.GetPrimaryKeys());
            await OnValueChangedAsync(entry);
        }
        else
        {
            var primaryKeys = args.SelectedModel.GetPrimaryKeys();
            await OnValueChangedAsync(primaryKeys[0]);
        }

        UpdateSelectedValue();
    }
}