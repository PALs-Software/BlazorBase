using BlazorBase.CRUD.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Components.BaseDisplayComponent;

namespace BlazorBase.CRUD.Models
{
    public interface IBaseModel
    {
        #region Events
        public event EventHandler<string> OnForcePropertyRepaint;

        public event EventHandler OnReloadEntityFromDatabase;
        #endregion

        #region Attribute Methods
        public List<PropertyInfo> GetVisibleProperties();

        public object[] GetPrimaryKeys();

        public string GetPrimaryKeysAsString();

        public Dictionary<string, string> GetNavigationQuery(string baseQuery = null);
        #endregion

        #region CRUD Methods
        public void ForcePropertyRepaint(string propertyName);
        public void ReloadEntityFromDatabase();

        #region Model

        #region DbContext
        public record OnBeforeDbContextAddEntryArgs(EventServices EventServices);
        public Task OnBeforeDbContextAddEntry(OnBeforeDbContextAddEntryArgs args);
        public record OnAfterDbContextAddedEntryArgs(EventServices EventServices);
        public Task OnAfterDbContextAddedEntry(OnAfterDbContextAddedEntryArgs args);

        public record OnBeforeDbContextModifyEntryArgs(EventServices EventServices);
        public Task OnBeforeDbContextModifyEntry(OnBeforeDbContextModifyEntryArgs args);
        public record OnAfterDbContextModifiedEntryArgs(EventServices EventServices);
        public Task OnAfterDbContextModifiedEntry(OnAfterDbContextModifiedEntryArgs args);


        public record OnBeforeDbContextDeleteEntryArgs(EventServices EventServices);
        public Task OnBeforeDbContextDeleteEntry(OnBeforeDbContextDeleteEntryArgs args);
        public record OnAfterDbContextDeletedEntryArgs(EventServices EventServices);
        public Task OnAfterDbContextDeletedEntry(OnAfterDbContextDeletedEntryArgs args);
        #endregion

        public record OnBeforeConvertPropertyTypeArgs(IBaseModel Model, string PropertyName, object NewValue, EventServices EventServices);
        public Task OnBeforeConvertPropertyType(OnBeforeConvertPropertyTypeArgs args);

        public record OnBeforePropertyChangedArgs(IBaseModel Model, string PropertyName, object NewValue, EventServices EventServices);
        public Task OnBeforePropertyChanged(OnBeforePropertyChangedArgs args);


        public record OnAfterPropertyChangedArgs(IBaseModel Model, string PropertyName, object NewValue, bool IsValid, EventServices EventServices);
        public Task OnAfterPropertyChanged(OnAfterPropertyChangedArgs args);


        public record OnCreateNewEntryInstanceArgs(IBaseModel Model, EventServices EventServices);
        public Task OnCreateNewEntryInstance(OnCreateNewEntryInstanceArgs args);


        public record OnBeforeAddEntryArgs(IBaseModel Model, bool AbortAdding, EventServices EventServices);
        public Task OnBeforeAddEntry(OnBeforeAddEntryArgs args);


        public record OnAfterAddEntryArgs(IBaseModel Model, EventServices EventServices);
        public Task OnAfterAddEntry(OnAfterAddEntryArgs args);


        public record OnBeforeUpdateEntryArgs(IBaseModel Model, bool AbortUpdating, EventServices EventServices);
        public Task OnBeforeUpdateEntry(OnBeforeUpdateEntryArgs args);


        public record OnAfterUpdateEntryArgs(IBaseModel Model, EventServices EventServices);
        public Task OnAfterUpdateEntry(OnAfterUpdateEntryArgs args);


        public record OnBeforeRemoveEntryArgs(IBaseModel Model, bool AbortRemoving, EventServices EventServices);
        public Task OnBeforeRemoveEntry(OnBeforeRemoveEntryArgs args);


        public record OnAfterRemoveEntryArgs(IBaseModel Model, EventServices EventServices);
        public Task OnAfterRemoveEntry(OnAfterRemoveEntryArgs args);


        public record OnAfterSaveChangesArgs(IBaseModel Model, bool NavigationProperty, EventServices EventServices);
        public Task OnAfterSaveChanges(OnAfterSaveChangesArgs args);
        #endregion

        #region ListProperty
        public record OnBeforeConvertListPropertyTypeArgs(IBaseModel Model, string PropertyName, object NewValue, EventServices EventServices);
        public Task OnBeforeConvertListPropertyType(OnBeforeConvertListPropertyTypeArgs args);


        public record OnBeforeListPropertyChangedArgs(IBaseModel Model, string PropertyName, object NewValue, EventServices EventServices);
        public Task OnBeforeListPropertyChanged(OnBeforeListPropertyChangedArgs args);


        public record OnAfterListPropertyChangedArgs(IBaseModel Model, string PropertyName, object NewValue, bool IsValid, EventServices EventServices);
        public Task OnAfterListPropertyChanged(OnAfterListPropertyChangedArgs args);

        public record OnCreateNewListEntryInstanceArgs(IBaseModel Model, object ListEntry, EventServices EventServices);
        public Task OnCreateNewListEntryInstance(OnCreateNewListEntryInstanceArgs args);

        public record OnBeforeAddListEntryArgs(IBaseModel Model, object ListEntry, bool AbortAdding, EventServices EventServices);
        public Task OnBeforeAddListEntry(OnBeforeAddListEntryArgs args);


        public record OnAfterAddListEntryArgs(IBaseModel Model, object ListEntry, EventServices EventServices);
        public Task OnAfterAddListEntry(OnAfterAddListEntryArgs args);


        public record OnBeforeUpdateListEntryArgs(IBaseModel Model, bool AbortUpdating, EventServices EventServices);
        public Task OnBeforeUpdateListEntry(OnBeforeUpdateListEntryArgs args);


        public record OnAfterUpdateListEntryArgs(IBaseModel Model, EventServices EventServices);
        public Task OnAfterUpdateListEntry(OnAfterUpdateListEntryArgs args);


        public record OnBeforeRemoveListEntryArgs(IBaseModel Model, object ListEntry, bool AbortRemoving, EventServices EventServices);
        public Task OnBeforeRemoveListEntry(OnBeforeRemoveListEntryArgs args);


        public record OnAfterRemoveListEntryArgs(IBaseModel Model, object ListEntry, EventServices EventServices);
        public Task OnAfterRemoveListEntry(OnAfterRemoveListEntryArgs args);
        #endregion

        #endregion

        #region Validation Methods
        public bool TryValidate(out List<ValidationResult> validationResults, ValidationContext validationContext);
        public bool TryValidateProperty(out List<ValidationResult> validationResults, ValidationContext propertyValidationContext, PropertyInfo propertyInfo);
        #endregion

        #region PageActions
        public List<PageActionGroup> GeneratePageActionGroups();
        #endregion

        #region Other
        public Type GetUnproxiedType();

        public void TransferPropertiesTo(object target);
        #endregion
    }
}
