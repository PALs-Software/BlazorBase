using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Components.Card;
using BlazorBase.CRUD.Components.List;
using BlazorBase.CRUD.Components.PageActions.Models;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Resources.ValidationAttributes;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.Extensions;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Models
{
    public class BaseModel : ComponentBase, IBaseModel
    {
        #region Events
        public event EventHandler<string[]>? OnForcePropertyRepaint;

        public event EventHandler? OnReloadEntityFromDatabase;

        public event EventHandler? OnRecalculateVisibilityStatesOfActions;
        #endregion

        public BaseModel()
        {
            CreatedOn = DateTime.Now;
            ModifiedOn = CreatedOn;
        }

        #region Additional Properties
        [Editable(false)]
        [DateDisplayMode(DateInputMode = DateInputMode.DateTime)]
        [Visible(DisplayGroup = "Information", DisplayGroupOrder = 9999, DisplayOrder = 100, Collapsed = true)]
        public virtual DateTime CreatedOn { get; set; }

        [Editable(false)]
        [DateDisplayMode(DateInputMode = DateInputMode.DateTime)]
        [Visible(DisplayGroup = "Information", DisplayOrder = 200)]
        public virtual DateTime ModifiedOn { get; set; }
        #endregion

        #region Configuration Properties
        [NotMapped] public virtual bool UserCanAddEntries { get; protected set; } = true;
        [NotMapped] public virtual bool UserCanEditEntries { get; protected set; } = true;
        [NotMapped] public virtual bool UserCanOpenCardReadOnly { get; protected set; } = false;
        [NotMapped] public virtual bool UserCanDeleteEntries { get; protected set; } = true;

        [NotMapped] public virtual bool StickyRowButtons { get; protected set; } = true;

        [NotMapped] public virtual List<Expression<Func<IBaseModel, bool>>>? DataLoadConditions { get; protected set; }
        [NotMapped] public virtual bool ShowOnlySingleEntry { get; protected set; }
        #endregion

        #region Attribute Methods
        public virtual List<PropertyInfo> GetVisibleProperties(GUIType guiType, List<string> userRoles)
        {
            return GetType().GetVisibleProperties(guiType, userRoles);
        }

        private object?[]? CachedPrimaryKeys = null;
        public object?[]? GetPrimaryKeys(bool useCache = false)
        {
            if (useCache && CachedPrimaryKeys != null)
                return CachedPrimaryKeys;

            var keyProperties = GetKeyProperties();

            var keys = new object?[keyProperties.Count];
            for (int i = 0; i < keyProperties.Count; i++)
                keys[i] = keyProperties.ElementAt(i).GetValue(this);

            if (useCache)
                CachedPrimaryKeys = keys;

            return keys;
        }

        public string GetPrimaryKeysAsString()
        {
            return String.Join(", ", GetPrimaryKeys() ?? Array.Empty<string>());
        }

        public List<PropertyInfo> GetKeyProperties()
        {
            return GetType().GetKeyProperties();
        }

        public Dictionary<string, string?> GetNavigationQuery(string? baseQuery = null)
        {
            var query = new Dictionary<string, string?>();
            if (baseQuery != null)
                query = QueryHelpers.ParseQuery(baseQuery).ToDictionary(key => key.Key, val => (string?)val.Value.ToString());

            var keyProperties = GetKeyProperties();

            var primaryKeys = new List<object>();
            foreach (var keyProperty in keyProperties)
                query[keyProperty.Name] = keyProperty.GetValue(this)?.ToString() ?? String.Empty;

            return query;
        }

        public string GetDisplayKey(string seperator)
        {
            var displayKeyProperties = GetType().GetDisplayKeyProperties();
            if (displayKeyProperties.Count == 0)
                return String.Join(seperator, GetPrimaryKeys() ?? Array.Empty<string>());
            else
                return GetDisplayKeyKeyValuePair(displayKeyProperties);
        }

        public string GetDisplayKey()
        {
            return GetDisplayKey(", ");
        }

        public string GetDisplayKeyKeyValuePair(List<PropertyInfo> displayKeyProperties)
        {
            var displayKeyValues = new List<object?>();
            foreach (var displayKeyProperty in displayKeyProperties)
            {
                var displayKeyValue = displayKeyProperty.GetValue(this);
                if (displayKeyValue is IBaseModel baseModel)
                    displayKeyValue = baseModel.GetDisplayKeyKeyValuePair(baseModel.GetType().GetDisplayKeyProperties());
                displayKeyValues.Add(displayKeyValue);
            }

            return String.Join(", ", displayKeyValues);
        }

        public bool PrimaryKeysAreEqual(object?[]? secondModelsPrimaryKeys, bool useCache = false)
        {
            var primaryKeys = GetPrimaryKeys(useCache);
            if (primaryKeys == null && secondModelsPrimaryKeys == null)
                return true;

            if (primaryKeys == null || secondModelsPrimaryKeys == null || primaryKeys.Length != secondModelsPrimaryKeys.Length)
                return false;

            for (int i = 0; i < primaryKeys.Length; i++)
                if (primaryKeys[i]?.GetHashCode() != secondModelsPrimaryKeys[i]?.GetHashCode())
                    return false;

            return true;
        }
        #endregion

        #region CRUD Methods
        public void ForcePropertyRepaint(params string[] propertyNames)
        {
            OnForcePropertyRepaint?.Invoke(this, propertyNames);
        }

        public void ReloadEntityFromDatabase()
        {
            OnReloadEntityFromDatabase?.Invoke(this, EventArgs.Empty);
        }

        public void RecalculateVisibilityStatesOfActions()
        {
            OnRecalculateVisibilityStatesOfActions?.Invoke(this, EventArgs.Empty);
        }

        public async Task InvokeStateHasChangedAsync()
        {
            await InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }
        #endregion

        #region Events

        #region DbContext Events
        public virtual Task OnBeforeDbContextAddEntry(OnBeforeDbContextAddEntryArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterDbContextAddedEntry(OnAfterDbContextAddedEntryArgs args) { return Task.CompletedTask; }

        public virtual Task OnBeforeDbContextModifyEntry(OnBeforeDbContextModifyEntryArgs args)
        {
            ModifiedOn = DateTime.Now;
            return Task.CompletedTask;
        }
        public virtual Task OnAfterDbContextModifiedEntry(OnAfterDbContextModifiedEntryArgs args) { return Task.CompletedTask; }

        public virtual Task OnBeforeDbContextDeleteEntry(OnBeforeDbContextDeleteEntryArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterDbContextDeletedEntry(OnAfterDbContextDeletedEntryArgs args) { return Task.CompletedTask; }
        #endregion

        #region Entry Events
        public virtual void OnGetPropertyCaption(OnGetPropertyCaptionArgs args) { }
        public virtual Task OnFormatProperty(OnFormatPropertyArgs args) { return Task.CompletedTask; }
        public virtual Task OnBeforeConvertPropertyType(OnBeforeConvertPropertyTypeArgs args) { return Task.CompletedTask; }
        public virtual Task OnBeforePropertyChanged(OnBeforePropertyChangedArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterPropertyChanged(OnAfterPropertyChangedArgs args) { return Task.CompletedTask; }
        public virtual Task OnCreateNewEntryInstance(OnCreateNewEntryInstanceArgs args) { return Task.CompletedTask; }
        public virtual Task OnBeforeAddEntry(OnBeforeAddEntryArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterAddEntry(OnAfterAddEntryArgs args) { return Task.CompletedTask; }
        public virtual Task OnBeforeUpdateEntry(OnBeforeUpdateEntryArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterUpdateEntry(OnAfterUpdateEntryArgs args) { return Task.CompletedTask; }
        public virtual Task OnBeforeRemoveEntry(OnBeforeRemoveEntryArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterRemoveEntry(OnAfterRemoveEntryArgs args) { return Task.CompletedTask; }
        public virtual Task OnBeforeCardSaveChanges(OnBeforeCardSaveChangesArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterCardSaveChanges(OnAfterCardSaveChangesArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterMoveEntryUp(OnAfterMoveEntryUpArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterMoveEntryDown(OnAfterMoveEntryDownArgs args) { return Task.CompletedTask; }
        #endregion

        #region List Events 
        public virtual Task OnBeforeConvertListPropertyType(OnBeforeConvertListPropertyTypeArgs args) { return Task.CompletedTask; }
        public virtual Task OnBeforeListPropertyChanged(OnBeforeListPropertyChangedArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterListPropertyChanged(OnAfterListPropertyChangedArgs args) { return Task.CompletedTask; }
        public virtual Task OnCreateNewListEntryInstance(OnCreateNewListEntryInstanceArgs args) { return Task.CompletedTask; }
        public virtual Task OnBeforeAddListEntry(OnBeforeAddListEntryArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterAddListEntry(OnAfterAddListEntryArgs args) { return Task.CompletedTask; }
        public virtual Task OnBeforeRemoveListEntry(OnBeforeRemoveListEntryArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterRemoveListEntry(OnAfterRemoveListEntryArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterMoveListEntryDown(OnAfterMoveListEntryDownArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterMoveListEntryUp(OnAfterMoveListEntryUpArgs args) { return Task.CompletedTask; }
        #endregion

        #region Data Loading

        public virtual void OnGuiLoadData(OnGuiLoadDataArgs args) { }

        #endregion

        #endregion

        #region Validation Methods
        public virtual Task OnBeforeValidateProperty(OnBeforeValidatePropertyArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterValidateProperty(OnAfterValidatePropertyArgs args) { return Task.CompletedTask; }

        public bool TryValidate(out List<ValidationResult> validationResults, ValidationContext validationContext)
        {
            validationResults = new List<ValidationResult>();
            return Validator.TryValidateObject(this, validationContext, validationResults, true);
        }

        public record ValidationTranslationResource(System.Resources.ResourceManager ResourceManager, Type ResourceType);
        public bool TryValidateProperty(out List<ValidationResult> validationResults, ValidationContext propertyValidationContext, PropertyInfo propertyInfo, List<ValidationAttribute>? additionalValidationAttributes = null, ValidationTranslationResource? translationResource = null)
        {
            validationResults = new List<ValidationResult>();
            var attributes = propertyInfo.GetCustomAttributes<Attribute>().OfType<ValidationAttribute>().ToList();

            if (additionalValidationAttributes != null)
                attributes.AddRange(additionalValidationAttributes);

            foreach (var attribute in attributes.Where(attribute => attribute.ErrorMessage == null))
            {
                var attributeName = attribute.GetType().Name;

                if (translationResource == null)
                    translationResource = new ValidationTranslationResource(ValidationAttributesTranslations.ResourceManager, typeof(ValidationAttributesTranslations));

                if (translationResource.ResourceManager.GetString(attributeName) != null)
                {
                    attribute.ErrorMessageResourceType = translationResource.ResourceType;
                    attribute.ErrorMessageResourceName = attributeName;
                }
            }

            return Validator.TryValidateValue(propertyInfo.GetValue(this)!, propertyValidationContext, validationResults, attributes);
        }

        public bool TryValidate(out List<ValidationResult> validationResults, EventServices eventServices)
        {
            var validationContext = new ValidationContext(this, eventServices.ServiceProvider, new Dictionary<object, object?>()
            {
                [typeof(IStringLocalizer)] = eventServices.Localizer,
                [typeof(BaseService)] = eventServices.BaseService
            });

            return TryValidate(out validationResults, validationContext);
        }

        #endregion

        #region ComponentBase
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            if (ShowOnlySingleEntry)
                BuildCardComponent(builder);
            else
                BuildListComponent(builder);
        }

        protected virtual void BuildListComponent(RenderTreeBuilder builder)
        {
            builder.OpenComponent(0, typeof(BaseList<>).MakeGenericType(GetType()));
            builder.AddAttribute(1, "UserCanAddEntries", UserCanAddEntries);
            builder.AddAttribute(2, "UserCanEditEntries", UserCanEditEntries);
            builder.AddAttribute(3, "UserCanOpenCardReadOnly", UserCanOpenCardReadOnly);
            builder.AddAttribute(4, "UserCanDeleteEntries", UserCanDeleteEntries);

            builder.AddAttribute(5, "StickyRowButtons", StickyRowButtons);

            builder.AddAttribute(6, "DataLoadConditions", DataLoadConditions);
            builder.AddAttribute(7, "ComponentModelInstance", this);
            builder.CloseComponent();
        }

        protected virtual void BuildCardComponent(RenderTreeBuilder builder)
        {
            builder.OpenComponent(0, typeof(BaseCard<>).MakeGenericType(GetType()));
            builder.AddAttribute(1, "ShowEntryByStart", ShowOnlySingleEntry);
            builder.AddAttribute(2, "EntryToBeShownByStart", new Func<OnEntryToBeShownByStartArgs, Task<IBaseModel?>>(GetShowOnlySingleEntryInstance));
            builder.CloseComponent();
        }

        public virtual async Task<IBaseModel?> GetShowOnlySingleEntryInstance(OnEntryToBeShownByStartArgs args)
        {
            return (IBaseModel?)await args.EventServices.BaseService.Set(GetType()).FirstOrDefaultAsync();
        }
        #endregion

        #region PageActions
        public virtual Task<List<PageActionGroup>?> GeneratePageActionGroupsAsync(EventServices eventServices) { return Task.FromResult<List<PageActionGroup>?>(null); }
        #endregion

        #region Helper Methods
        public void ClearPropertyValues()
        {
            var properties = GetType().GetPropertiesExceptKeys().Where(property =>
                !typeof(IBaseModel).IsAssignableFrom(property.PropertyType) &&
                !typeof(ILazyLoader).IsAssignableFrom(property.PropertyType) &&
                property.CanWrite &&
                property.GetSetMethod() != null &&
                (property.GetSetMethod()!.Attributes & MethodAttributes.Static) == 0 &&
                property.Name != nameof(CreatedOn) &&
                property.Name != nameof(ModifiedOn)
            ).ToList();

            foreach (var property in properties)
                property.SetValue(this, default);
        }

        public void TransferPropertiesExceptKeysTo(object target, params string[] exceptPropertyNames)
        {
            var sourceProperties = this.GetType().GetPropertiesExceptKeys().Where(property => !exceptPropertyNames.Contains(property.Name));
            TransferPropertiesTo(target, sourceProperties.ToArray());
        }

        public void TransferPropertiesTo(object target, params string[] exceptPropertyNames)
        {
            var sourceProperties = this.GetType().GetProperties().Where(property => !exceptPropertyNames.Contains(property.Name));
            TransferPropertiesTo(target, sourceProperties.ToArray());
        }

        public void TransferPropertiesTo(object target, PropertyInfo[]? sourceProperties = null)
        {
            ObjectExtension.TransferPropertiesTo(this, target, sourceProperties);
        }

        public Type GetUnproxiedType()
        {
            var type = GetType();

            if (type.Namespace == "Castle.Proxies" && type.BaseType != null)
                return type.BaseType;

            return type;
        }

        #endregion       
    }
}
