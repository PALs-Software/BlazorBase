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
        protected string InitialSelectedValue;
        protected SelectList<KeyValuePair<string, string>, string> SelectList = default;
        protected bool IsForeignKeyProperty = false;
        protected bool InputReferencesBaseModelWithMultiplePrimaryKeys = false;
        protected List<PropertyInfo> ForeignKeyProperties;
        #endregion


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            InitialSelectedValue = Property.GetValue(Model)?.ToString();

            var foreignKey = Property.GetCustomAttribute(typeof(ForeignKeyAttribute)) as ForeignKeyAttribute;
            IsForeignKeyProperty = foreignKey != null;

            if (IsForeignKeyProperty && foreignKey.Name.Contains(","))
            {
                var value = Property.GetValue(Model);
                if (value != null)
                    InitialSelectedValue = JsonConvert.SerializeObject(((IBaseModel)value).GetPrimaryKeys());

                ForeignKeyProperties = Property.PropertyType.GetKeyProperties();
                InputReferencesBaseModelWithMultiplePrimaryKeys = true;
            }
            else if (IsForeignKeyProperty)
                InitialSelectedValue = JsonConvert.SerializeObject(new object[] { InitialSelectedValue });
        }

        protected async virtual Task OnSelectedValueChangedAsync(string selectedValue)
        {
            if (!IsForeignKeyProperty || selectedValue == null)
            {
                await OnValueChangedAsync(selectedValue);
                return;
            }

            var primaryKeys = JsonConvert.DeserializeObject<object[]>(selectedValue);
            if (!InputReferencesBaseModelWithMultiplePrimaryKeys)
            {
                await OnValueChangedAsync(primaryKeys[0]);
                return;
            }

            for (int i = 0; i < primaryKeys.Length; i++)
                primaryKeys[i] = Convert.ChangeType(primaryKeys[i], ForeignKeyProperties[i].PropertyType, CultureInfo.InvariantCulture);

            var entry = await Service.GetAsync(Property.PropertyType, primaryKeys);
            await OnValueChangedAsync(entry);
        }

        protected virtual string DisplaySelectValue()
        {
            var key = Property.GetValue(Model)?.ToString();
            if (key == null)
                return String.Empty;

            if (IsForeignKeyProperty)
                key = JsonConvert.SerializeObject(new object[] { key });
            var dataKeyValuePair = Data.FirstOrDefault(entry => entry.Key == key);

            if (dataKeyValuePair.Equals(default(KeyValuePair<string, string>)))
                return key?.ToString() ?? String.Empty;
            else
                return dataKeyValuePair.Value?.ToString() ?? String.Empty;
        }
    }
}
