using BlazorBase.Abstractions.CRUD.Arguments;
using BlazorBase.Abstractions.CRUD.Enums;
using BlazorBase.Abstractions.CRUD.Structures;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazorBase.Abstractions.CRUD.Interfaces;

public interface IBaseModel
{
    #region Events
    event EventHandler<string[]>? OnForcePropertyRepaint;

    event EventHandler? OnReloadEntityFromDatabase;

    event EventHandler? OnRecalculateVisibilityStatesOfActions;
    event EventHandler<string[]>? OnRecalculateCustomLookupData;
    #endregion

    #region Attribute Methods
    List<PropertyInfo> GetVisibleProperties(GUIType guiType, List<string> userRoles);

    object?[]? GetPrimaryKeys(bool useCache = false);

    string GetPrimaryKeysAsString();

    Dictionary<string, string?> GetNavigationQuery(string? baseQuery = null);

    string GetDisplayKey();

    string GetDisplayKey(string seperator);

    string GetDisplayKeyKeyValuePair(List<PropertyInfo> displayKeyProperties);

    bool PrimaryKeysAreEqual(object?[]? secondModelsPrimaryKeys, bool useCache = false);
    #endregion

    #region CRUD Methods
    void ForcePropertyRepaint(params string[] propertyNames);
    void ReloadEntityFromDatabase();

    void RecalculateVisibilityStatesOfActions();
    void RecalculateCustomLookupData(params string[] propertyNames);
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
    Task OnBeforeCardSaveChanges(OnBeforeCardSaveChangesArgs args);
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

    #region Data Loading

    void OnGuiLoadData(OnGuiLoadDataArgs args);
    Task OnShowEntry(OnShowEntryArgs args);

    #endregion

    #endregion

    #region Validation Methods
    Task OnBeforeValidateProperty(OnBeforeValidatePropertyArgs args);
    Task OnAfterValidateProperty(OnAfterValidatePropertyArgs args);

    bool TryValidate(out List<ValidationResult> validationResults, ValidationContext validationContext);

    public record ValidationTranslationResource(System.Resources.ResourceManager ResourceManager, Type ResourceType);
    bool TryValidateProperty(out List<ValidationResult> validationResults, ValidationContext propertyValidationContext, PropertyInfo propertyInfo, List<ValidationAttribute>? additionalValidationAttributes = null, ValidationTranslationResource? translationResource = null);
    #endregion

    #region PageActions
    Task<List<PageActionGroup>?> GeneratePageActionGroupsAsync(EventServices eventServices);
    #endregion

    #region ComponentBase        
    bool UserCanAddEntries { get; }
    bool UserCanEditEntries { get; }
    bool UserCanOpenCardReadOnly { get; }
    bool UserCanDeleteEntries { get; }
    List<Expression<Func<IBaseModel, bool>>>? DataLoadConditions { get; }
    bool ShowOnlySingleEntry { get; }
    Task<IBaseModel?> GetShowOnlySingleEntryInstance(OnEntryToBeShownByStartArgs args);
    #endregion
           
    #region Helper Methods
    void ClearPropertyValues();
    Type GetUnproxiedType();
    void TransferPropertiesExceptKeysTo(object target, params string[] exceptPropertyNames);
    void TransferPropertiesTo(object target, PropertyInfo[]? sourceProperties = null);
    #endregion

    #region Caption Methods
    static string? GetPropertyCaption(EventServices eventServices, IBaseModel model, IStringLocalizer modelLocalizer, IDisplayItem displayItem)
    {
        var args = new OnGetPropertyCaptionArgs(model, displayItem, modelLocalizer[displayItem.Property.Name], eventServices);
        model.OnGetPropertyCaption(args);

        return args.Caption;
    }

    static string GetPropertyTooltip(IStringLocalizer modelLocalizer, IDisplayItem displayItem)
    {
        var caption = modelLocalizer[displayItem.Property.Name];
        var tooltip = modelLocalizer[$"{displayItem.Property.Name}_Tooltip"];

        if (tooltip.Value != $"{displayItem.Property.Name}_Tooltip")
            return $"{caption.Value}{Environment.NewLine}{Environment.NewLine}{tooltip.Value}";

        return caption.Value;
    }

    static bool GetFieldHelpCaption(IStringLocalizer modelLocalizer, IDisplayItem displayItem, out string caption)
    {
        caption = modelLocalizer[$"{displayItem.Property.Name}_FieldHelp"];

        return caption != $"{displayItem.Property.Name}_FieldHelp";
    }
    #endregion
}
