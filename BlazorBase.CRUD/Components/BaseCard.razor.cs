using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Modules;
using BlazorBase.CRUD.Services;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseCard<TModel> where TModel : BaseModel, new()
    {
        [Parameter]
        public EventCallback OnCardClosed { get; set; }

        [Parameter]
        public EventCallback<TModel> OnEntryAdded { get; set; }

        [Parameter]
        public BaseService Service { get; set; }

        [Inject]
        private IStringLocalizer<TModel> ModelLocalizer { get; set; }

        [Inject]
        private IStringLocalizer<BaseCard<TModel>> Localizer { get; set; }


        private string Title;
        private string CardSummaryInvalidFeedback;
        private Modal Modal = default!;
        private TModel Entry;
        private Type TModelType;

        private bool AddingMode;

        private List<PropertyInfo> VisibleProperties = new List<PropertyInfo>();
        private Dictionary<PropertyInfo, Dictionary<string, string>> ForeignKeyProperties;
        private List<BaseInput> BaseInputs = new List<BaseInput>();
        private List<BaseInputSelectList> BaseInputSelectLists = new List<BaseInputSelectList>();

        BaseInput AddToBaseInputs { set { BaseInputs.Add(value); } }
        BaseInputSelectList AddToBaseInputSelectLists { set { BaseInputSelectLists.Add(value); } }

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                TModelType = typeof(TModel);
                VisibleProperties = TModelType.GetVisibleProperties(GUIType.Card);

                Title = Localizer["TitleEdit", ModelLocalizer[nameof(TModel)]];
            });
        }

        public async Task PrepareForeignKeyProperties()
        {
            if (ForeignKeyProperties != null)
                return;

            ForeignKeyProperties = new Dictionary<PropertyInfo, Dictionary<string, string>>();

            var foreignKeyProperties = VisibleProperties.Where(entry => entry.IsForeignKey());
            foreach (var foreignKeyProperty in foreignKeyProperties)
            {
                var foreignKey = foreignKeyProperty.GetCustomAttribute(typeof(ForeignKeyAttribute)) as ForeignKeyAttribute;
                var type = TModelType.GetProperty(foreignKey.Name).PropertyType;
                var displayKeyProperty = type.GetDisplayKeyProperty();

                var entries = (await Service.GetDataAsync(type));
                var primaryKeys = new Dictionary<string, string>();

                primaryKeys.Add(BaseConstants.GenericNullString, "");
                foreach (var entry in entries)
                {
                    var primaryKeysAsString = ((BaseModel) entry).GetPrimaryKeysAsString();
                    if (displayKeyProperty == null)
                        primaryKeys.Add(primaryKeysAsString, primaryKeysAsString);
                    else
                        primaryKeys.Add(primaryKeysAsString, displayKeyProperty.GetValue(entry).ToString());
                }

                ForeignKeyProperties.Add(foreignKeyProperty, primaryKeys);
            }
        }

        public async Task Show(TModel entry, bool addingMode = false)
        {
            await PrepareForeignKeyProperties();
            AddingMode = addingMode;
            Entry = entry;
            Modal.Show();
        }

        protected virtual bool CardIsValid()
        {
            foreach (var input in BaseInputs)
                input.ValidatePropertyValue();

            return Entry.TryValidate(out List<ValidationResult> validationResults);
        }

        protected async Task SaveModal()
        {
            CardSummaryInvalidFeedback = String.Empty;

            if (!CardIsValid())
                return;

            try
            {
                if (AddingMode)
                {
                    if (!await Entry.OnBeforeAddEntry(Service.DbContext))
                        return;

                    if (!await Service.AddEntry(Entry))
                    {
                        CardSummaryInvalidFeedback = Localizer["EntryAlreadyExistError",Entry.GetPrimaryKeysAsString()];
                        return;
                    }

                    await Entry.OnAfterAddEntry(Service.DbContext);
                    await OnEntryAdded.InvokeAsync(Entry);
                }
                else
                {
                    if (!await Entry.OnBeforeUpdateEntry(Service.DbContext))
                        return;

                    Service.UpdateEntry(Entry);
                    await Entry.OnAfterUpdateEntry(Service.DbContext);
                }

                await Service.SaveChangesAsync();
            }
            catch (Exception e)
            {
                CardSummaryInvalidFeedback = Localizer["UnknownSavingError", e.Message];
                return;
            }

            Modal.Hide();
            await OnCardClosed.InvokeAsync(null);
            Entry = null;
        }

        protected async Task RejectModal()
        {
            if (!AddingMode)
                await Service.ReloadAsync(Entry);

            Modal.Hide();
            await OnCardClosed.InvokeAsync(null);
            Entry = null;
        }
    }
}
