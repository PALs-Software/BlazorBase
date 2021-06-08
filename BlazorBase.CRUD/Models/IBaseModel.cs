using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
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

        public string GetDisplayKey();

        public string GetDisplayKeyKeyValuePair(List<PropertyInfo> displayKeyProperties);
        #endregion

        #region CRUD Methods
        public void ForcePropertyRepaint(string propertyName);
        public void ReloadEntityFromDatabase();
        #endregion

        #region Base Model Events

        #region DbContext Events
        public Task OnBeforeDbContextAddEntry(OnBeforeDbContextAddEntryArgs args);
        public Task OnAfterDbContextAddedEntry(OnAfterDbContextAddedEntryArgs args);
        public Task OnBeforeDbContextModifyEntry(OnBeforeDbContextModifyEntryArgs args);
        public Task OnAfterDbContextModifiedEntry(OnAfterDbContextModifiedEntryArgs args);
        public Task OnBeforeDbContextDeleteEntry(OnBeforeDbContextDeleteEntryArgs args);
        public Task OnAfterDbContextDeletedEntry(OnAfterDbContextDeletedEntryArgs args);
        #endregion

        #region Entry Events
        public void OnGetPropertyCaption(OnGetPropertyCaptionArgs args);
        public Task OnBeforeConvertPropertyType(OnBeforeConvertPropertyTypeArgs args);
        public Task OnBeforePropertyChanged(OnBeforePropertyChangedArgs args);
        public Task OnAfterPropertyChanged(OnAfterPropertyChangedArgs args);
        public Task OnCreateNewEntryInstance(OnCreateNewEntryInstanceArgs args);
        public Task OnBeforeAddEntry(OnBeforeAddEntryArgs args);
        public Task OnAfterAddEntry(OnAfterAddEntryArgs args);
        public Task OnBeforeUpdateEntry(OnBeforeUpdateEntryArgs args);
        public Task OnAfterUpdateEntry(OnAfterUpdateEntryArgs args);
        public Task OnBeforeRemoveEntry(OnBeforeRemoveEntryArgs args);
        public Task OnAfterRemoveEntry(OnAfterRemoveEntryArgs args);
        public Task OnAfterCardSaveChanges(OnAfterCardSaveChangesArgs args);
        public Task OnAfterMoveEntryUp(OnAfterMoveEntryUpArgs args);
        public Task OnAfterMoveEntryDown(OnAfterMoveEntryDownArgs args);
        #endregion

        #region List Property Events
        public Task OnBeforeConvertListPropertyType(OnBeforeConvertListPropertyTypeArgs args);
        public Task OnBeforeListPropertyChanged(OnBeforeListPropertyChangedArgs args);
        public Task OnAfterListPropertyChanged(OnAfterListPropertyChangedArgs args);
        public Task OnCreateNewListEntryInstance(OnCreateNewListEntryInstanceArgs args);
        public Task OnBeforeAddListEntry(OnBeforeAddListEntryArgs args);
        public Task OnAfterAddListEntry(OnAfterAddListEntryArgs args);
        public Task OnBeforeRemoveListEntry(OnBeforeRemoveListEntryArgs args);
        public Task OnAfterRemoveListEntry(OnAfterRemoveListEntryArgs args);
        public Task OnAfterMoveListEntryUp(OnAfterMoveListEntryUpArgs args);
        public Task OnAfterMoveListEntryDown(OnAfterMoveListEntryDownArgs args);
        #endregion

        #endregion

        #region Validation Methods
        public bool TryValidate(out List<ValidationResult> validationResults, ValidationContext validationContext);
        public bool TryValidateProperty(out List<ValidationResult> validationResults, ValidationContext propertyValidationContext, PropertyInfo propertyInfo);
        public bool CheckIfModelIsInAddingMode(BaseService baseService);
        #endregion

        #region PageActions
        public List<PageActionGroup> GeneratePageActionGroups();
        #endregion

        #region ComponentBase
        bool UserCanAddEntries { get; }
        bool UserCanEditEntries { get; }
        bool UserCanDeleteEntries { get; }
        Expression<Func<IBaseModel, bool>> DataLoadCondition { get; }
        bool ShowOnlySingleEntry { get; }
        Task<IBaseModel> GetShowOnlySingleEntryInstance(EventServices eventServices);
        #endregion

        #region Other
        public Type GetUnproxiedType();
        public void TransferPropertiesExceptKeysTo(object target, params string[] exceptPropertyNames);
        public void TransferPropertiesTo(object target, PropertyInfo[] sourceProperties = null);
        #endregion
    }
}
