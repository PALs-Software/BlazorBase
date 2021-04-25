﻿using BlazorBase.Components;
using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.MessageHandling.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        [Parameter] public EventCallback<OnCreateNewListEntryInstanceArgs> OnCreateNewListEntryInstance { get; set; }
        [Parameter] public EventCallback<OnBeforeAddListEntryArgs> OnBeforeAddListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterAddListEntryArgs> OnAfterAddListEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeUpdateListEntryArgs> OnBeforeUpdateListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterUpdateListEntryArgs> OnAfterUpdateListEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeConvertListPropertyTypeArgs> OnBeforeConvertListPropertyType { get; set; }
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
        [Inject] protected StringLocalizerFactory GenericClassStringLocalizer { get; set; }
        [Inject] protected IStringLocalizer<BaseListPart> Localizer { get; set; }
        [Inject] protected IServiceProvider ServiceProvider { get; set; }
        [Inject] protected IMessageHandler MessageHandler { get; set; }
        #endregion

        #region Members
        protected IStringLocalizer ModelLocalizer { get; set; }
        protected Type IStringModelLocalizerType { get; set; }
        protected IList Entries { get; set; }
        protected Type ModelListEntryType { get; set; }

        #region Property Infos
        protected List<BaseInput> BaseInputs = new List<BaseInput>();
        protected List<BaseSelectListInput> BaseSelectListInputs = new List<BaseSelectListInput>();
        protected List<IBasePropertyListPartInput> BasePropertyListPartInputs = new List<IBasePropertyListPartInput>();

        protected BaseInput AddToBaseInputs { set { BaseInputs.Add(value); } }
        protected BaseSelectListInput AddToBaseInputSelectLists { set { BaseSelectListInputs.Add(value); } }

        protected List<IBasePropertyListPartInput> BaseInputExtensions = new List<IBasePropertyListPartInput>();
        #endregion
        #endregion

        #region Init

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                ModelListEntryType = Property.GetCustomAttribute<RenderTypeAttribute>()?.RenderType ?? Property.PropertyType.GenericTypeArguments[0];

                IStringModelLocalizerType = typeof(IStringLocalizer<>).MakeGenericType(Model.GetUnproxiedType());
                ModelLocalizer = GenericClassStringLocalizer.GetLocalizer(ModelListEntryType);

                SetUpDisplayLists(ModelListEntryType, GUIType.ListPart);

                if (String.IsNullOrEmpty(SingleDisplayName))
                    SingleDisplayName = ModelLocalizer[ModelListEntryType.Name];
                if (String.IsNullOrEmpty(PluralDisplayName))
                    PluralDisplayName = ModelLocalizer[$"{ModelListEntryType.Name}_Plural"];

                if (Property.GetValue(Model) == null)
                    Property.SetValue(Model, CreateGenericListInstance());

                BaseInputExtensions = ServiceProvider.GetServices<IBasePropertyListPartInput>().ToList();

                Entries = (IList)Property.GetValue(Model);
            });

            await PrepareForeignKeyProperties(ModelListEntryType, Service);
        }

        protected async Task<RenderFragment> CheckIfPropertyRenderingIsHandledAsync(DisplayItem displayItem, IBaseModel model)
        {
            var eventServices = GetEventServices();

            foreach (var baseinput in BaseInputExtensions)
                if (await baseinput.IsHandlingPropertyRenderingAsync(model, displayItem, eventServices))
                    return GetBaseInputExtensionAsRenderFragment(displayItem, baseinput.GetType(), model);

            return null;
        }

        protected RenderFragment GetBaseInputExtensionAsRenderFragment(DisplayItem displayItem, Type baseInputExtensionType, IBaseModel model) => builder =>
        {
            builder.OpenComponent(0, baseInputExtensionType);

            builder.AddAttribute(1, "Model", model);
            builder.AddAttribute(2, "Property", displayItem.Property);
            builder.AddAttribute(3, "ReadOnly", displayItem.Property.IsKey());
            builder.AddAttribute(4, "Service", Service);
            builder.AddAttribute(5, "ModelLocalizer", ModelLocalizer);

            builder.AddAttribute(6, "OnBeforeConvertPropertyType", EventCallback.Factory.Create<OnBeforeConvertPropertyTypeArgs>(this, (args) => OnBeforeConvertListPropertyType.InvokeAsync(new OnBeforeConvertListPropertyTypeArgs(args.Model, args.PropertyName, args.NewValue, args.EventServices))));
            builder.AddAttribute(7, "OnBeforePropertyChanged", EventCallback.Factory.Create<OnBeforePropertyChangedArgs>(this, (args) => OnBeforeListPropertyChanged.InvokeAsync(new OnBeforeListPropertyChangedArgs(args.Model, args.PropertyName, args.NewValue, args.EventServices))));
            builder.AddAttribute(8, "OnAfterPropertyChanged", EventCallback.Factory.Create<OnAfterPropertyChangedArgs>(this, (args) => OnAfterListPropertyChanged.InvokeAsync(new OnAfterListPropertyChangedArgs(args.Model, args.PropertyName, args.NewValue, args.IsValid, args.EventServices))));

            builder.AddComponentReferenceCapture(9, (input) => BasePropertyListPartInputs.Add((IBasePropertyListPartInput)input));

            builder.CloseComponent();
        };
        #endregion

        #region CRUD
        protected object CreateGenericListInstance()
        {
            var listType = typeof(List<>);
            var constructedListType = listType.MakeGenericType(Property.PropertyType.GenericTypeArguments[0]);
            return Activator.CreateInstance(constructedListType);
        }

        protected async Task AddEntryAsync()
        {
            var newEntry = Activator.CreateInstance(ModelListEntryType);
            await OnCreateNewListEntryInstanceAsync(newEntry);

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
        protected async Task OnCreateNewListEntryInstanceAsync(object newEntry)
        {
            var eventServices = GetEventServices();

            var onCreateNewListEntryInstanceArgs = new OnCreateNewListEntryInstanceArgs(Model, newEntry, eventServices);
            await OnCreateNewListEntryInstance.InvokeAsync(onCreateNewListEntryInstanceArgs);

            if (newEntry is not IBaseModel newBaseEntry)
                return;

            var onCreateNewEntryInstanceArgs = new OnCreateNewEntryInstanceArgs(Model, eventServices);
            await newBaseEntry.OnCreateNewEntryInstance(onCreateNewEntryInstanceArgs);
        }


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

        #region Validation
        public virtual bool ListPartIsValid()
        {
            var valid = true;

            foreach (var input in BaseInputs)
                if (!input.ValidatePropertyValue())
                    valid = false;

            foreach (var input in BaseSelectListInputs)
                if (!input.ValidatePropertyValue())
                    valid = false;

            foreach (var basePropertyListPartInput in BasePropertyListPartInputs)
                if (!basePropertyListPartInput.ValidatePropertyValue())
                    valid = false;

            foreach (var item in Entries)
            {
                if (item is IBaseModel baseModel)
                {
                    var validationContext = new ValidationContext(item, ServiceProvider, new Dictionary<object, object>()
                    {
                        [IStringModelLocalizerType] = ModelLocalizer,
                        [typeof(DbContext)] = Service.DbContext
                    });

                    if (!baseModel.TryValidate(out List<ValidationResult> validationResults, validationContext))
                        valid = false;
                }
            }

            return valid;
        }
        #endregion
        #region Other       
        protected EventServices GetEventServices()
        {
            return new EventServices()
            {
                ServiceProvider = ServiceProvider,
                Localizer = ModelLocalizer,
                BaseService = Service,
                MessageHandler = MessageHandler
            };
        }
        #endregion
    }
}
