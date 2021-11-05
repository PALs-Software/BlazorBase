using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        event EventHandler<string> OnForcePropertyRepaint;

        event EventHandler OnReloadEntityFromDatabase;
        #endregion

        #region Attribute Methods
        List<PropertyInfo> GetVisibleProperties(GUIType? guiType = null);

        object[] GetPrimaryKeys();

        string GetPrimaryKeysAsString();

        Dictionary<string, string> GetNavigationQuery(string baseQuery = null);

        string GetDisplayKey();

        string GetDisplayKeyKeyValuePair(List<PropertyInfo> displayKeyProperties);
        #endregion

        #region CRUD Methods
        void ForcePropertyRepaint(string propertyName);
        void ReloadEntityFromDatabase();
        #endregion

        #region Base Model Events

        #region DbContext Events
        Task OnBeforeDbContextAddEntry(OnBeforeDbContextAddEntryArgs args);
        Task OnAfterDbContextAddedEntry(OnAfterDbContextAddedEntryArgs args);
        Task OnBeforeDbContextModifyEntry(OnBeforeDbContextModifyEntryArgs args);
        Task OnAfterDbContextModifiedEntry(OnAfterDbContextModifiedEntryArgs args);
        Task OnBeforeDbContextDeleteEntry(OnBeforeDbContextDeleteEntryArgs args);
        Task OnAfterDbContextDeletedEntry(OnAfterDbContextDeletedEntryArgs args);
        #endregion

        #region Entry Events
        void OnGetPropertyCaption(OnGetPropertyCaptionArgs args);
        Task OnFormatProperty(OnFormatPropertyArgs args);
        Task OnBeforeConvertPropertyType(OnBeforeConvertPropertyTypeArgs args);
        Task OnBeforePropertyChanged(OnBeforePropertyChangedArgs args);
        Task OnAfterPropertyChanged(OnAfterPropertyChangedArgs args);
        Task OnCreateNewEntryInstance(OnCreateNewEntryInstanceArgs args);
        Task OnBeforeAddEntry(OnBeforeAddEntryArgs args);
        Task OnAfterAddEntry(OnAfterAddEntryArgs args);
        Task OnBeforeUpdateEntry(OnBeforeUpdateEntryArgs args);
        Task OnAfterUpdateEntry(OnAfterUpdateEntryArgs args);
        Task OnBeforeRemoveEntry(OnBeforeRemoveEntryArgs args);
        Task OnAfterRemoveEntry(OnAfterRemoveEntryArgs args);
        Task OnAfterCardSaveChanges(OnAfterCardSaveChangesArgs args);
        Task OnAfterMoveEntryUp(OnAfterMoveEntryUpArgs args);
        Task OnAfterMoveEntryDown(OnAfterMoveEntryDownArgs args);
        #endregion

        #region List Property Events
        Task OnBeforeConvertListPropertyType(OnBeforeConvertListPropertyTypeArgs args);
        Task OnBeforeListPropertyChanged(OnBeforeListPropertyChangedArgs args);
        Task OnAfterListPropertyChanged(OnAfterListPropertyChangedArgs args);
        Task OnCreateNewListEntryInstance(OnCreateNewListEntryInstanceArgs args);
        Task OnBeforeAddListEntry(OnBeforeAddListEntryArgs args);
        Task OnAfterAddListEntry(OnAfterAddListEntryArgs args);
        Task OnBeforeRemoveListEntry(OnBeforeRemoveListEntryArgs args);
        Task OnAfterRemoveListEntry(OnAfterRemoveListEntryArgs args);
        Task OnAfterMoveListEntryUp(OnAfterMoveListEntryUpArgs args);
        Task OnAfterMoveListEntryDown(OnAfterMoveListEntryDownArgs args);
        #endregion

        #endregion

        #region Validation Methods
        bool TryValidate(out List<ValidationResult> validationResults, ValidationContext validationContext);
        bool TryValidateProperty(out List<ValidationResult> validationResults, ValidationContext propertyValidationContext, PropertyInfo propertyInfo);
        bool CheckIfModelIsInAddingMode(BaseService baseService);
        #endregion

        #region PageActions
        List<PageActionGroup> GeneratePageActionGroups();
        #endregion

        #region ComponentBase        
        bool UserCanAddEntries { get; }
        bool UserCanEditEntries { get; }
        bool UserCanDeleteEntries { get; }
        Expression<Func<IBaseModel, bool>> DataLoadCondition { get; }
        bool ShowOnlySingleEntry { get; }
        Task<IBaseModel> GetShowOnlySingleEntryInstance(EventServices eventServices);
        List<string> PropertyNamesToRemoveFromListView { get; set; }
        #endregion

        #region Helper Methods
        void ClearPropertyValues();
        Type GetUnproxiedType();
        void TransferPropertiesExceptKeysTo(object target, params string[] exceptPropertyNames);
        void TransferPropertiesTo(object target, PropertyInfo[] sourceProperties = null);
        #endregion
    }
}
