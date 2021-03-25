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
using static BlazorBase.CRUD.Models.IBaseModel;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseCard<TModel> : BaseDisplayComponent where TModel : class, IBaseModel, new()
    {
        #region Parameter

        #region Events
        [Parameter] public EventCallback OnCardClosed { get; set; }
        [Parameter] public EventCallback<OnBeforeAddEntryArgs> OnBeforeAddEntry { get; set; }
        [Parameter] public EventCallback<OnAfterAddEntryArgs> OnAfterAddEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeUpdateEntryArgs> OnBeforeUpdateEntry { get; set; }
        [Parameter] public EventCallback<OnAfterUpdateEntryArgs> OnAfterUpdateEntry { get; set; }
        [Parameter] public EventCallback<OnBeforePropertyChangedArgs> OnBeforePropertyChanged { get; set; }
        [Parameter] public EventCallback<OnAfterPropertyChangedArgs> OnAfterPropertyChanged { get; set; }

        #region List Events
        [Parameter] public EventCallback<OnBeforeAddListEntryArgs> OnBeforeAddListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterAddListEntryArgs> OnAfterAddListEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeUpdateListEntryArgs> OnBeforeUpdateListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterUpdateListEntryArgs> OnAfterUpdateListEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeListPropertyChangedArgs> OnBeforeListPropertyChanged { get; set; }
        [Parameter] public EventCallback<OnAfterListPropertyChangedArgs> OnAfterListPropertyChanged { get; set; }
        [Parameter] public EventCallback<OnBeforeRemoveListEntryArgs> OnBeforeRemoveListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterRemoveListEntryArgs> OnAfterRemoveListEntry { get; set; }
        #endregion
        #endregion

        [Inject]
        public BaseService Service { get; set; }

        [Parameter]
        public string SingleDisplayName { get; set; }

        #endregion

        #region Injects

        [Inject]
        private IStringLocalizer<TModel> ModelLocalizer { get; set; }

        [Inject]
        private StringLocalizerFactory GenericClassStringLocalizer { get; set; }

        private IStringLocalizer Localizer { get; set; }

        [Inject]
        private IServiceProvider ServiceProvider { get; set; }
        #endregion

        #region Member
        private string Title;
        private string CardSummaryInvalidFeedback;
        private bool ShowInvalidFeedback = false;
        private Modal Modal = default!;
        private TModel Entry;
        private Type TModelType;

        private bool AddingMode;

        protected ValidationContext ValidationContext;
        #endregion

        #region Property Infos

        protected Dictionary<PropertyInfo, Dictionary<string, string>> ForeignKeyProperties;

        protected List<BaseInput<TModel>> BaseInputs = new List<BaseInput<TModel>>();
        protected List<BaseInputSelectList<TModel>> BaseInputSelectLists = new List<BaseInputSelectList<TModel>>();

        protected BaseInput<TModel> AddToBaseInputs { set { BaseInputs.Add(value); } }
        protected BaseInputSelectList<TModel> AddToBaseInputSelectLists { set { BaseInputSelectLists.Add(value); } }

        #endregion

        #region Init
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                Localizer = GenericClassStringLocalizer.GetLocalizer(typeof(BaseCard<TModel>));
                TModelType = typeof(TModel);

                if (String.IsNullOrEmpty(SingleDisplayName))
                    SingleDisplayName = ModelLocalizer[TModelType.Name];
                Title = Localizer[nameof(Title), SingleDisplayName];

                SetUpDisplayLists(TModelType, GUIType.Card);
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
        #endregion

        #region Modal
        public async Task Show(bool addingMode = false, params object[] primaryKeys)
        {
            Service.RefreshDbContext();
            Service.DbContext.Database.BeginTransaction();

            await PrepareForeignKeyProperties();
            AddingMode = addingMode;

            if (AddingMode)
                Entry = new TModel();
            else
                Entry = await Service.GetAsync<TModel>(primaryKeys);

            if (Entry == null)
                throw new CRUDException(Localizer["Can not find Entry with the Primarykeys {0} for displaying in Card", String.Join(", ", primaryKeys)]);

            BaseInputs.Clear();
            BaseInputSelectLists.Clear();

            ValidationContext = new ValidationContext(Entry, ServiceProvider, new Dictionary<object, object>()
            {
                [typeof(IStringLocalizer<TModel>)] = ModelLocalizer,
                [typeof(DbContext)] = Service.DbContext
            });

            Modal.Show();
        }

        protected async Task SaveModal()
        {
            CardSummaryInvalidFeedback = String.Empty;
            ShowInvalidFeedback = false;

            if (!CardIsValid())
                return;

            var eventServices = GetEventServices();

            try
            {
                if (AddingMode)
                {
                    var args = new OnBeforeAddEntryArgs(Entry, false, eventServices);
                    await OnBeforeAddEntry.InvokeAsync(args);
                    await Entry.OnBeforeAddEntry(args);
                    if (args.AbortAdding)
                        return;

                    if (!await Service.AddEntryAsync(Entry))
                    {
                        CardSummaryInvalidFeedback = Localizer["EntryAlreadyExistError", Entry.GetPrimaryKeysAsString()];
                        return;
                    }

                    var onAfterArgs = new OnAfterAddEntryArgs(Entry, eventServices);
                    await OnAfterAddEntry.InvokeAsync(onAfterArgs);
                    await Entry.OnAfterAddEntry(onAfterArgs);
                }
                else
                {
                    var args = new OnBeforeUpdateEntryArgs(Entry, false, eventServices);
                    await OnBeforeUpdateEntry.InvokeAsync(args);
                    await Entry.OnBeforeUpdateEntry(args);
                    if (args.AbortUpdating)
                        return;

                    Service.UpdateEntry(Entry);

                    var onAfterArgs = new OnAfterUpdateEntryArgs(Entry, eventServices);
                    await OnAfterUpdateEntry.InvokeAsync(onAfterArgs);
                    await Entry.OnAfterUpdateEntry(onAfterArgs);
                }

                await Service.SaveChangesAsync();
                Service.DbContext.Database.CommitTransaction();
            }
            catch (CRUDException e)
            {
                CardSummaryInvalidFeedback = e.Message;
                ShowInvalidFeedback = true;
                return;
            }
            catch (Exception e)
            {
                CardSummaryInvalidFeedback = Localizer["UnknownSavingError", e.Message];
                ShowInvalidFeedback = true;
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
        #endregion

        #region Validation
        protected virtual bool CardIsValid()
        {
            foreach (var input in BaseInputs)
                input.ValidatePropertyValue();

            foreach (var input in BaseInputSelectLists)
                input.ValidatePropertyValue();

            return Entry.TryValidate(out List<ValidationResult> validationResults, ValidationContext);
        }
        #endregion

        #region Other
        private EventServices GetEventServices()
        {
            return new EventServices()
            {
                ServiceProvider = ServiceProvider,
                Localizer = ModelLocalizer,
                BaseService = Service
            };
        }

        private Dictionary<string, string> GetEnumValueDictionary(Type enumType)
        {
            var result = new Dictionary<string, string>();
            var values = Enum.GetNames(enumType);
            var localizer = GenericClassStringLocalizer.GetLocalizer(enumType);
            foreach (var value in values)
                result.Add(value, localizer[value]);

            return result;
        }

        #endregion
    }
}
