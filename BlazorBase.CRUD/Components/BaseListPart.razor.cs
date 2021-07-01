using BlazorBase.Components;
using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.SortableItem;
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

namespace BlazorBase.CRUD.Components
{
    public partial class BaseListPart : BaseDisplayComponent
    {
        #region Parameters

        #region Events
        [Parameter] public EventCallback<OnCreateNewListEntryInstanceArgs> OnCreateNewListEntryInstance { get; set; }
        [Parameter] public EventCallback<OnBeforeAddListEntryArgs> OnBeforeAddListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterAddListEntryArgs> OnAfterAddListEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeConvertListPropertyTypeArgs> OnBeforeConvertListPropertyType { get; set; }
        [Parameter] public EventCallback<OnBeforeListPropertyChangedArgs> OnBeforeListPropertyChanged { get; set; }
        [Parameter] public EventCallback<OnAfterListPropertyChangedArgs> OnAfterListPropertyChanged { get; set; }
        [Parameter] public EventCallback<OnBeforeRemoveListEntryArgs> OnBeforeRemoveListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterRemoveListEntryArgs> OnAfterRemoveListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterMoveListEntryUpArgs> OnAfterMoveListEntryUp { get; set; }
        [Parameter] public EventCallback<OnAfterMoveListEntryDownArgs> OnAfterMoveListEntryDown { get; set; }
        #endregion

        [Parameter] public IBaseModel Model { get; set; }
        [Parameter] public PropertyInfo Property { get; set; }
        [Parameter] public BaseService Service { get; set; }
        [Parameter] public bool? ReadOnly { get; set; }
        [Parameter] public string SingleDisplayName { get; set; }
        [Parameter] public string PluralDisplayName { get; set; }
        #endregion

        #region Injects
        [Inject] protected IStringLocalizer<BaseListPart> Localizer { get; set; }
        [Inject] protected IServiceProvider ServiceProvider { get; set; }
        [Inject] protected IMessageHandler MessageHandler { get; set; }
        #endregion

        #region Members
        protected EventServices EventServices;

        protected IStringLocalizer ModelLocalizer { get; set; }
        protected Type IStringModelLocalizerType { get; set; }
        protected IList Entries { get; set; }
        protected Type ModelListEntryType { get; set; }

        protected bool ModelImplementedISortableItem { get; set; }
        protected SortableItemComparer SortableItemComparer { get; set; } = new SortableItemComparer();

        #region Property Infos
        protected bool IsReadOnly;
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
                EventServices = GetEventServices();

                ModelListEntryType = Property.GetCustomAttribute<RenderTypeAttribute>()?.RenderType ?? Property.PropertyType.GenericTypeArguments[0];
                ModelImplementedISortableItem = ModelListEntryType.ImplementedISortableItem();

                IStringModelLocalizerType = typeof(IStringLocalizer<>).MakeGenericType(Property.PropertyType.GenericTypeArguments[0]);
                ModelLocalizer = StringLocalizerFactory.Create(Property.PropertyType.GenericTypeArguments[0]);

                if (ReadOnly == null)
                    IsReadOnly = Property.IsReadOnlyInGUI();
                else
                    IsReadOnly = ReadOnly.Value;

                SetUpDisplayLists(ModelListEntryType, GUIType.ListPart);

                if (String.IsNullOrEmpty(SingleDisplayName))
                    SingleDisplayName = ModelLocalizer[ModelListEntryType.Name];
                if (String.IsNullOrEmpty(PluralDisplayName))
                    PluralDisplayName = ModelLocalizer[$"{ModelListEntryType.Name}_Plural"];

                if (Property.GetValue(Model) == null)
                    Property.SetValue(Model, CreateGenericListInstance());

                BaseInputExtensions = ServiceProvider.GetServices<IBasePropertyListPartInput>().ToList();

                dynamic entries = Property.GetValue(Model);
                if (ModelImplementedISortableItem)
                    entries.Sort(SortableItemComparer);
                Entries = (IList)entries;

                Model.OnForcePropertyRepaint += Model_OnForcePropertyRepaint;
            });

            await PrepareForeignKeyProperties(Service);
            await PrepareCustomLookupData(Model, EventServices);
        }

        protected override void OnParametersSet()
        {
            if (ReadOnly == null)
                IsReadOnly = Property.IsReadOnlyInGUI();
            else
                IsReadOnly = ReadOnly.Value;
        }

        private void Model_OnForcePropertyRepaint(object sender, string propertyName)
        {
            if (propertyName != Property.Name)
                return;

            InvokeAsync(() => StateHasChanged());
        }

        protected async Task<RenderFragment> CheckIfPropertyRenderingIsHandledAsync(DisplayItem displayItem, bool isReadonly, IBaseModel model)
        {
            foreach (var baseinput in BaseInputExtensions)
                if (await baseinput.IsHandlingPropertyRenderingAsync(model, displayItem, EventServices))
                    return GetBaseInputExtensionAsRenderFragment(displayItem, isReadonly, baseinput.GetType(), model);

            return null;
        }

        protected virtual RenderFragment GetBaseInputExtensionAsRenderFragment(DisplayItem displayItem, bool isReadonly, Type baseInputExtensionType, IBaseModel model) => builder =>
        {
            builder.OpenComponent(0, baseInputExtensionType);

            builder.AddAttribute(1, "Model", model);
            builder.AddAttribute(2, "Property", displayItem.Property);
            builder.AddAttribute(3, "ReadOnly", isReadonly);
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

        protected async Task AddEntryAsync(object aboveEntry = null)
        {
            var newEntry = Activator.CreateInstance(ModelListEntryType);
            await OnCreateNewListEntryInstanceAsync(newEntry);

            var args = new HandledEventArgs();
            await OnBeforeAddEntryAsync(newEntry, args);
            if (args.Handled)
                return;

            if (ModelImplementedISortableItem && aboveEntry != null)
                Entries.Insert(Entries.IndexOf(aboveEntry), newEntry);
            else
                Entries.Add(newEntry);

            SetSortIndex();

            await OnAfterAddEntryAsync(newEntry);
        }

        protected async Task RemoveEntryAsync(object entry)
        {
            var args = new HandledEventArgs();
            await OnBeforeRemoveEntryAsync(entry, args);
            if (args.Handled)
                return;

            Entries.Remove(entry);
            BaseInputs.RemoveAll(input => input.Model == entry);
            BaseSelectListInputs.RemoveAll(input => input.Model == entry);
            BasePropertyListPartInputs.RemoveAll(input => input.Model == entry);

            await OnAfterRemoveEntryAsync(entry);
        }

        protected async Task MoveEntryUpAsync(object entry)
        {
            if (!ModelImplementedISortableItem)
                return;

            var index = Entries.IndexOf(entry);
            if (index == 0)
                return;

            SwapEntries(entry, index, index - 1);
            SetSortIndex();

            await OnAfterMoveListEntryUpAsync(entry);
        }

        protected async Task MoveEntryDownAsync(object entry)
        {
            if (!ModelImplementedISortableItem)
                return;

            var index = Entries.IndexOf(entry);
            if (index == Entries.Count - 1)
                return;

            SwapEntries(entry, index, index + 1);
            SetSortIndex();

            await OnAfterMoveListEntryDownAsync(entry);
        }

        protected void SwapEntries(object entry, int currentIndex, int targetIndex)
        {
            var tempEntry = Entries[targetIndex];
            Entries[targetIndex] = entry;
            Entries[currentIndex] = tempEntry;
        }

        protected void SetSortIndex()
        {
            if (!ModelImplementedISortableItem)
                return;

            for (int index = 0; index < Entries.Count; index++)
                (Entries[index] as ISortableItem).SortIndex = index;
        }
        #endregion

        #region Events
        protected async Task OnCreateNewListEntryInstanceAsync(object newEntry)
        {
            var onCreateNewListEntryInstanceArgs = new OnCreateNewListEntryInstanceArgs(Model, newEntry, EventServices);
            await OnCreateNewListEntryInstance.InvokeAsync(onCreateNewListEntryInstanceArgs);
            await Model.OnCreateNewListEntryInstance(onCreateNewListEntryInstanceArgs);

            if (newEntry is not IBaseModel newBaseEntry)
                return;

            var onCreateNewEntryInstanceArgs = new OnCreateNewEntryInstanceArgs(Model, EventServices);
            await newBaseEntry.OnCreateNewEntryInstance(onCreateNewEntryInstanceArgs);
        }

        protected async Task OnBeforeAddEntryAsync(object newEntry, HandledEventArgs args)
        {
            var onBeforeAddListEntryArgs = new OnBeforeAddListEntryArgs(Model, newEntry, false, EventServices);
            await OnBeforeAddListEntry.InvokeAsync(onBeforeAddListEntryArgs);
            await Model.OnBeforeAddListEntry(onBeforeAddListEntryArgs);
            if (onBeforeAddListEntryArgs.AbortAdding)
            {
                args.Handled = true;
                return;
            }

            if (newEntry is IBaseModel newBaseEntry)
            {
                var onBeforeAddEntryArgs = new OnBeforeAddEntryArgs(Model, false, EventServices);
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
            var onAfterAddListEntryArgs = new OnAfterAddListEntryArgs(Model, newEntry, EventServices);
            await OnAfterAddListEntry.InvokeAsync(onAfterAddListEntryArgs);
            await Model.OnAfterAddListEntry(onAfterAddListEntryArgs);

            if (newEntry is IBaseModel addedBaseEntry)
                await addedBaseEntry.OnAfterAddEntry(new OnAfterAddEntryArgs(Model, EventServices));
        }

        protected async Task OnBeforeRemoveEntryAsync(object entry, HandledEventArgs args)
        {
            var onBeforeRemoveListEntry = new OnBeforeRemoveListEntryArgs(Model, entry, false, EventServices);
            await OnBeforeRemoveListEntry.InvokeAsync(onBeforeRemoveListEntry);
            await Model.OnBeforeRemoveListEntry(onBeforeRemoveListEntry);
            if (onBeforeRemoveListEntry.AbortRemoving)
            {
                args.Handled = true;
                return;
            }

            if (entry is IBaseModel newBaseEntry)
            {
                var onBeforeRemoveEntryArgs = new OnBeforeRemoveEntryArgs(Model, false, EventServices);
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
            var onAfterRemoveListEntryArgs = new OnAfterRemoveListEntryArgs(Model, entry, EventServices);
            await OnAfterRemoveListEntry.InvokeAsync(onAfterRemoveListEntryArgs);
            await Model.OnAfterRemoveListEntry(onAfterRemoveListEntryArgs);

            if (entry is IBaseModel RemovedBaseEntry)
                await RemovedBaseEntry.OnAfterRemoveEntry(new OnAfterRemoveEntryArgs(Model, EventServices));
        }

        protected async Task OnAfterMoveListEntryUpAsync(object entry)
        {
            var args = new OnAfterMoveListEntryUpArgs(Model, entry, EventServices);
            await OnAfterMoveListEntryUp.InvokeAsync(args);
            await Model.OnAfterMoveListEntryUp(args);

            if (entry is IBaseModel baseModel)
                await baseModel.OnAfterMoveEntryUp(new OnAfterMoveEntryUpArgs(Model, EventServices));
        }

        protected async Task OnAfterMoveListEntryDownAsync(object entry)
        {
            var args = new OnAfterMoveListEntryDownArgs(Model, entry, EventServices);
            await OnAfterMoveListEntryDown.InvokeAsync(args);
            await Model.OnAfterMoveListEntryDown(args);

            if (entry is IBaseModel baseModel)
                await baseModel.OnAfterMoveEntryDown(new OnAfterMoveEntryDownArgs(Model, EventServices));
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
                        [typeof(IStringLocalizer)] = ModelLocalizer,
                        [typeof(BaseService)] = Service
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
