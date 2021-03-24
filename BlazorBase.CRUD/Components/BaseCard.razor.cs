using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Modules;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
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
    public partial class BaseCard<TModel> where TModel : class, IBaseModel, new()
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
        private GenericClassStringLocalizer GenericClassStringLocalizer { get; set; }

        private IStringLocalizer Localizer { get; set; }

        [Inject]
        private IServiceProvider ServiceProvider { get; set; }


        private string Title;
        private string CardSummaryInvalidFeedback;
        private Modal Modal = default!;
        private TModel Entry;
        private Type TModelType;

        private bool AddingMode;

        private List<PropertyInfo> VisibleProperties = new List<PropertyInfo>();
        private Dictionary<string, List<(VisibleAttribute Attribute, PropertyInfo Property)>> DisplayGroups = new Dictionary<string, List<(VisibleAttribute Attribute, PropertyInfo Property)>>();
        private Dictionary<PropertyInfo, Dictionary<string, string>> ForeignKeyProperties;
        private List<BaseInput<TModel>> BaseInputs = new List<BaseInput<TModel>>();
        private List<BaseInputSelectList<TModel>> BaseInputSelectLists = new List<BaseInputSelectList<TModel>>();

        BaseInput<TModel> AddToBaseInputs { set { BaseInputs.Add(value); } }
        BaseInputSelectList<TModel> AddToBaseInputSelectLists { set { BaseInputSelectLists.Add(value); } }
        protected ValidationContext ValidationContext;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                Localizer = GenericClassStringLocalizer.GetLocalizer(typeof(BaseCard<TModel>));
                TModelType = typeof(TModel);
                Title = Localizer[nameof(Title), ModelLocalizer[TModelType.Name]];

                VisibleProperties = TModelType.GetVisibleProperties(GUIType.Card);
                foreach (var property in VisibleProperties)
                {
                    var attribute = property.GetCustomAttributes(typeof(VisibleAttribute)).First() as VisibleAttribute;
                    attribute.DisplayGroup = String.IsNullOrEmpty(attribute.DisplayGroup) ? "General" : attribute.DisplayGroup;

                    if (!DisplayGroups.ContainsKey(attribute.DisplayGroup))
                        DisplayGroups[attribute.DisplayGroup] = new List<(VisibleAttribute Attribute, PropertyInfo Property)>();

                    DisplayGroups[attribute.DisplayGroup].Add((attribute, property));
                }

                DisplayGroups = DisplayGroups.OrderBy(entry => entry.Value.FirstOrDefault().Attribute.DisplayGroupOrder).ToDictionary(x => x.Key, x => x.Value);
                foreach (var properties in DisplayGroups)
                    properties.Value.Sort((x, y) => x.Attribute.DisplayOrder.CompareTo(y.Attribute.DisplayOrder));
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
                    var primaryKeysAsString = ((IBaseModel)entry).GetPrimaryKeysAsString();
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

            BaseInputs.Clear();
            BaseInputSelectLists.Clear();

            ValidationContext = new ValidationContext(Entry, ServiceProvider, new Dictionary<object, object>()
            {
                [typeof(IStringLocalizer<TModel>)] = ModelLocalizer,
                [typeof(DbContext)] = Service.DbContext
            });

            Modal.Show();
        }

        protected virtual bool CardIsValid()
        {
            foreach (var input in BaseInputs)
                input.ValidatePropertyValue();

            return Entry.TryValidate(out List<ValidationResult> validationResults, ValidationContext);
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
                    if (!await Entry.OnBeforeAddEntry(GetEventServices()))
                        return;

                    if (!await Service.AddEntryAsync(Entry))
                    {
                        CardSummaryInvalidFeedback = Localizer["EntryAlreadyExistError", Entry.GetPrimaryKeysAsString()];
                        return;
                    }

                    await Entry.OnAfterAddEntry(GetEventServices());
                    await OnEntryAdded.InvokeAsync(Entry);
                }
                else
                {
                    if (!await Entry.OnBeforeUpdateEntry(GetEventServices()))
                        return;

                    Service.UpdateEntry(Entry);
                    await Entry.OnAfterUpdateEntry(GetEventServices());
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

        private EventServices GetEventServices()
        {
            return new EventServices()
            {
                ServiceProvider = ServiceProvider,
                Localizer = ModelLocalizer,
                Service = Service
            };
        }
    }
}
