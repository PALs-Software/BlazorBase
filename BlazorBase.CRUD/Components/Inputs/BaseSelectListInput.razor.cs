using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Blazorise;
using BlazorBase.CRUD.Models;
using System.Threading.Tasks;
using Blazorise.Components;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System;
using BlazorBase.CRUD.Extensions;
using System.Globalization;
using static BlazorBase.CRUD.Components.General.BaseDisplayComponent;
using System.Linq;

namespace BlazorBase.CRUD.Components.Inputs
{
    public partial class BaseSelectListInput : BaseInput
    {
        #region Parameter
        [Parameter] public List<KeyValuePair<string, string>> Data { get; set; }
        #endregion

        #region Member
        protected string SelectedValueAsString;
        protected SelectList<KeyValuePair<string, string>, string> SelectList = default;
        protected bool IsForeignKeyProperty = false;
        protected bool InputReferencesBaseModelWithMultiplePrimaryKeys = false;
        protected List<PropertyInfo> ForeignKeyProperties;
        #endregion


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            var foreignKey = Property.GetCustomAttribute(typeof(ForeignKeyAttribute)) as ForeignKeyAttribute;
            IsForeignKeyProperty = foreignKey != null;

            if (IsForeignKeyProperty && foreignKey.Name.Contains(","))
            {
                ForeignKeyProperties = Property.PropertyType.GetKeyProperties();
                InputReferencesBaseModelWithMultiplePrimaryKeys = true;
            }

            UpdateSelectedValueFromModel();
        }

        protected async virtual Task OnSelectedValueChangedAsync(string selectedValue)
        {
            if (!IsForeignKeyProperty || selectedValue == null)
            {
                await OnValueChangedAsync(selectedValue);
                UpdateSelectedValueFromModel();
                return;
            }

            var primaryKeys = JsonConvert.DeserializeObject<object[]>(selectedValue);
            if (!InputReferencesBaseModelWithMultiplePrimaryKeys)
            {
                await OnValueChangedAsync(primaryKeys[0]);
                UpdateSelectedValueFromModel();
                return;
            }

            for (int i = 0; i < primaryKeys.Length; i++)
                primaryKeys[i] = Convert.ChangeType(primaryKeys[i], ForeignKeyProperties[i].PropertyType, CultureInfo.InvariantCulture);

            var entry = await Service.GetAsync(Property.PropertyType, primaryKeys);
            await OnValueChangedAsync(entry);
            UpdateSelectedValueFromModel();
        }

        protected virtual string DisplaySelectValue()
        {
            var key = Property.GetValue(Model);
            if (key == null)
                return String.Empty;

            string searchKey;
            if (IsForeignKeyProperty)
                searchKey = JsonConvert.SerializeObject(new object[] { key });
            else
                searchKey = key.ToString();

            var dataKeyValuePair = Data.FirstOrDefault(entry => entry.Key == searchKey);

            if (dataKeyValuePair.Equals(default(KeyValuePair<string, string>)))
                return key?.ToString() ?? String.Empty;
            else
                return dataKeyValuePair.Value?.ToString() ?? String.Empty;
        }

        protected void UpdateSelectedValueFromModel()
        {
            SelectedValueAsString = null;
            var value = Property.GetValue(Model);
            if (InputReferencesBaseModelWithMultiplePrimaryKeys)
            {
                if (value != null)
                    SelectedValueAsString = JsonConvert.SerializeObject(((IBaseModel)value).GetPrimaryKeys());
            }
            else if (IsForeignKeyProperty)
                SelectedValueAsString = JsonConvert.SerializeObject(new object[] { SelectedValueAsString });
            else
                SelectedValueAsString = value?.ToString();
        }
    }
}
