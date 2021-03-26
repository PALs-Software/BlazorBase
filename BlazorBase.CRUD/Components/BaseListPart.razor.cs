using BlazorBase.Components;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.MessageHandling.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Models.IBaseModel;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseListPart : BaseDisplayComponent
    {
        #region Parameters

        #region Events
        [Parameter] public EventCallback<OnBeforeAddListEntryArgs> OnBeforeAddListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterAddListEntryArgs> OnAfterAddListEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeUpdateListEntryArgs> OnBeforeUpdateListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterUpdateListEntryArgs> OnAfterUpdateListEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeListPropertyChangedArgs> OnBeforeListPropertyChanged { get; set; }
        [Parameter] public EventCallback<OnAfterListPropertyChangedArgs> OnAfterListPropertyChanged { get; set; }
        [Parameter] public EventCallback<OnBeforeRemoveListEntryArgs> OnBeforeRemoveListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterRemoveListEntryArgs> OnAfterRemoveListEntry { get; set; }
        #endregion

        [Parameter] public IBaseModel Model { get; set; }
        [Parameter] public PropertyInfo Property { get; set; }
        [Parameter] public BaseService Service { get; set; }

        [Parameter] public string SingleDisplayName { get; set; }
        [Parameter] public string PluralDisplayName { get; set; }
        #endregion

        #region Injects

        [Inject]
        private StringLocalizerFactory GenericClassStringLocalizer { get; set; }

        [Inject]
        private IStringLocalizer<BaseListPart> Localizer { get; set; }

        [Inject]
        private IServiceProvider ServiceProvider { get; set; }

        [CascadingParameter]
        protected IMessageHandler MessageHandler { get; set; }

        #endregion

        #region Members
        protected IStringLocalizer ModelLocalizer { get; set; }
        protected Type IStringModelLocalizerType { get; set; }
        protected IList Entries { get; set; }
        protected Type ModelListEntryType { get; set; }

        #region Property Infos
        protected Dictionary<PropertyInfo, Dictionary<string, string>> ForeignKeyProperties;

        protected List<BaseInput> BaseInputs = new List<BaseInput>();
        protected List<BaseInputSelectList> BaseInputSelectLists = new List<BaseInputSelectList>();

        protected BaseInput AddToBaseInputs { set { BaseInputs.Add(value); } }
        protected BaseInputSelectList AddToBaseInputSelectLists { set { BaseInputSelectLists.Add(value); } }
        #endregion
        #endregion

        #region Init

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                ModelListEntryType = Property.PropertyType.GenericTypeArguments[0];

                ModelLocalizer = GenericClassStringLocalizer.GetLocalizer(ModelListEntryType);
                

                SetUpDisplayLists(ModelListEntryType, GUIType.ListPart);

                if (String.IsNullOrEmpty(SingleDisplayName))
                    SingleDisplayName = ModelLocalizer[ModelListEntryType.Name];
                if (String.IsNullOrEmpty(PluralDisplayName))
                    PluralDisplayName = ModelLocalizer[$"{ModelListEntryType.Name}_Plural"];

                if (Property.GetValue(Model) == null)
                    Property.SetValue(Model, CreateGenericListInstance());
                Entries = (IList)Property.GetValue(Model);
            });
        }
        #endregion

        #region CRUD
        protected object CreateGenericListInstance()
        {
            var listType = typeof(List<>);
            var constructedListType = listType.MakeGenericType(ModelListEntryType);
            return Activator.CreateInstance(constructedListType);
        }

        protected async Task AddEntryAsync()
        {
            var newEntry = Activator.CreateInstance(ModelListEntryType);

            var args = new HandledEventArgs();
            await OnBeforeAddEntryAsync(newEntry, args);
            if (args.Handled)
                return;

            Entries.Add(newEntry);

            await OnAfterAddEntryAsync(newEntry);
        }

        protected async Task RemoveEntryAsync(object entry)
        {
            var args = new HandledEventArgs();
            await OnBeforeRemoveEntryAsync(entry, args);
            if (args.Handled)
                return;

            Entries.Remove(entry);

            await OnAfterRemoveEntryAsync(entry);
        }
        #endregion

        #region Events
        protected async Task OnBeforeAddEntryAsync(object newEntry, HandledEventArgs args)
        {
            var eventServices = GetEventServices();

            var onBeforeAddListEntryArgs = new OnBeforeAddListEntryArgs(Model, newEntry, false, eventServices);
            await OnBeforeAddListEntry.InvokeAsync(onBeforeAddListEntryArgs);
            await Model.OnBeforeAddListEntry(onBeforeAddListEntryArgs);
            if (onBeforeAddListEntryArgs.AbortAdding)
            {
                args.Handled = true;
                return;
            }

            if (newEntry is IBaseModel newBaseEntry)
            {
                var onBeforeAddEntryArgs = new OnBeforeAddEntryArgs(Model, false, eventServices);
                await newBaseEntry.OnBeforeAddEntry(onBeforeAddEntryArgs);
                if (onBeforeAddEntryArgs.AbortAdding)
                {
                    args.Handled = true;
                    return;
                }
            }

        }
        
        protected async Task OnAfterAddEntryAsync(object newEntry)
        {
            var eventServices = GetEventServices();

            var onAfterAddListEntryArgs = new OnAfterAddListEntryArgs(Model, newEntry, eventServices);
            await OnAfterAddListEntry.InvokeAsync(onAfterAddListEntryArgs);
            await Model.OnAfterAddListEntry(onAfterAddListEntryArgs);

            if (newEntry is IBaseModel addedBaseEntry)
                await addedBaseEntry.OnAfterAddEntry(new OnAfterAddEntryArgs(Model, eventServices));
        }

        protected async Task OnBeforeRemoveEntryAsync(object entry, HandledEventArgs args)
        {
            var eventServices = GetEventServices();

            var onBeforeRemoveListEntry = new OnBeforeRemoveListEntryArgs(Model, entry, false, eventServices);
            await OnBeforeRemoveListEntry.InvokeAsync(onBeforeRemoveListEntry);
            await Model.OnBeforeRemoveListEntry(onBeforeRemoveListEntry);
            if (onBeforeRemoveListEntry.AbortRemoving)
            {
                args.Handled = true;
                return;
            }

            if (entry is IBaseModel newBaseEntry)
            {
                var onBeforeRemoveEntryArgs = new OnBeforeRemoveEntryArgs(Model, false, eventServices);
                await newBaseEntry.OnBeforeRemoveEntry(onBeforeRemoveEntryArgs);
                if (onBeforeRemoveEntryArgs.AbortRemoving)
                {
                    args.Handled = true;
                    return;
                }
            }

        }

        protected async Task OnAfterRemoveEntryAsync(object entry)
        {
            var eventServices = GetEventServices();

            var onAfterRemoveListEntryArgs = new OnAfterRemoveListEntryArgs(Model, entry, eventServices);
            await OnAfterRemoveListEntry.InvokeAsync(onAfterRemoveListEntryArgs);
            await Model.OnAfterRemoveListEntry(onAfterRemoveListEntryArgs);

            if (entry is IBaseModel RemovedBaseEntry)
                await RemovedBaseEntry.OnAfterRemoveEntry(new OnAfterRemoveEntryArgs(Model, eventServices));
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
        #endregion
    }
}
