using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ViewModels;
using System.Collections.Generic;
using static BlazorBase.CRUD.Components.General.BaseDisplayComponent;

namespace BlazorBase.CRUD.EventArguments
{
    #region DbContext
    public record OnBeforeDbContextAddEntryArgs(EventServices EventServices);
    public record OnAfterDbContextAddedEntryArgs(EventServices EventServices);
    public record OnBeforeDbContextModifyEntryArgs(EventServices EventServices);
    public record OnAfterDbContextModifiedEntryArgs(EventServices EventServices);
    public record OnBeforeDbContextDeleteEntryArgs(EventServices EventServices);
    public record OnAfterDbContextDeletedEntryArgs(EventServices EventServices);
    #endregion

    #region Entry Event Args
    public record OnGetPropertyCaptionArgs(IBaseModel Model, DisplayItem DisplayItem, EventServices EventServices)
    {
        public OnGetPropertyCaptionArgs(IBaseModel model, DisplayItem displayItem, string caption, EventServices eventServices) : this(model, displayItem, eventServices) => Caption = caption;
        public string Caption { get; set; }
    }
    public record OnFormatPropertyArgs(IBaseModel Model, string PropertyName, Dictionary<string, object> InputAttributes, EventServices EventServices)
    {
        public OnFormatPropertyArgs(IBaseModel model, string propertyName, Dictionary<string, object> inputAttributes, string feedbackClass, string inputClass, string feedback, EventServices eventServices) : this(model, propertyName, inputAttributes, eventServices)
        {
            FeedbackClass = feedbackClass;
            InputClass = inputClass;
            Feedback = feedback;
        }

        public string FeedbackClass { get; set; }
        public string InputClass { get; set; }
        public string Feedback { get; set; }
    }
    public record OnBeforeConvertPropertyTypeArgs(IBaseModel Model, string PropertyName, object OldValue, EventServices EventServices)
    {
        public OnBeforeConvertPropertyTypeArgs(IBaseModel model, string propertyName, object newValue, object oldValue, EventServices eventServices) : this(model, propertyName, oldValue, eventServices) => NewValue = newValue;
        public object NewValue { get; set; }
    }
    public record OnBeforePropertyChangedArgs(IBaseModel Model, string PropertyName, object OldValue, EventServices EventServices)
    {
        public OnBeforePropertyChangedArgs(IBaseModel model, string propertyName, object newValue, object oldValue, EventServices eventServices) : this(model, propertyName, oldValue, eventServices) => NewValue = newValue;
        public object NewValue { get; set; }
    }
    public record OnAfterPropertyChangedArgs(IBaseModel Model, string PropertyName, object NewValue, object OldValue, bool IsValid, EventServices EventServices);
    public record OnCreateNewEntryInstanceArgs(IBaseModel Model, EventServices EventServices);
    public record OnBeforeAddEntryArgs(IBaseModel Model, EventServices EventServices)
    {
        public OnBeforeAddEntryArgs(IBaseModel model, bool abortAdding, EventServices eventServices) : this(model, eventServices) => AbortAdding = abortAdding;
        public bool AbortAdding { get; set; }
    }
    public record OnAfterAddEntryArgs(IBaseModel Model, EventServices EventServices);
    public record OnBeforeUpdateEntryArgs(IBaseModel Model, EventServices EventServices)
    {
        public OnBeforeUpdateEntryArgs(IBaseModel model, bool abortUpdating, EventServices eventServices) : this(model, eventServices) => AbortUpdating = abortUpdating;
        public bool AbortUpdating { get; set; }
    }
    public record OnAfterUpdateEntryArgs(IBaseModel Model, EventServices EventServices);
    public record OnBeforeRemoveEntryArgs(IBaseModel Model, EventServices EventServices)
    {
        public OnBeforeRemoveEntryArgs(IBaseModel model, bool abortRemoving, EventServices eventServices) : this(model, eventServices) => AbortRemoving = abortRemoving;
        public bool AbortRemoving { get; set; }
    }
    public record OnAfterRemoveEntryArgs(IBaseModel Model, EventServices EventServices);
    public record OnBeforeCardSaveChangesArgs(IBaseModel Model, bool IsNavigationProperty, EventServices EventServices);
    public record OnAfterCardSaveChangesArgs(IBaseModel Model, bool IsNavigationProperty, EventServices EventServices);
    public record OnAfterMoveEntryUpArgs(IBaseModel Model, EventServices EventServices);
    public record OnAfterMoveEntryDownArgs(IBaseModel Model, EventServices EventServices);

    #endregion

    #region Validation
    public record OnBeforeValidatePropertyArgs(IBaseModel Model, string PropertyName, EventServices EventServices)
    {
        public bool IsValid { get; set; }
        public bool IsHandled { get; set; }
        public string ErrorMessage { get; set; }
    }
    public record OnAfterValidatePropertyArgs(IBaseModel Model, string PropertyName, EventServices EventServices)
    {
        public OnAfterValidatePropertyArgs(IBaseModel model, string propertyName, EventServices eventServices, bool isValid, string errorMessage) : this(model, propertyName, eventServices)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }
    #endregion

    #region List Property Event Args
    public record OnBeforeConvertListPropertyTypeArgs(IBaseModel Model, string PropertyName, object OldValue, EventServices EventServices)
    {
        public OnBeforeConvertListPropertyTypeArgs(IBaseModel model, string propertyName, object newValue, object oldValue, EventServices eventServices) : this(model, propertyName, oldValue, eventServices) => NewValue = newValue;
        public object NewValue { get; set; }
    };
    public record OnBeforeListPropertyChangedArgs(IBaseModel Model, string PropertyName, object OldValue, EventServices EventServices)
    {
        public OnBeforeListPropertyChangedArgs(IBaseModel model, string propertyName, object newValue, object oldValue, EventServices eventServices) : this(model, propertyName, oldValue, eventServices) => NewValue = newValue;
        public object NewValue { get; set; }
    }
    public record OnAfterListPropertyChangedArgs(IBaseModel Model, string PropertyName, object NewValue, object OldValue, bool IsValid, EventServices EventServices);
    public record OnCreateNewListEntryInstanceArgs(IBaseModel Model, object ListEntry, EventServices EventServices);
    public record OnBeforeAddListEntryArgs(IBaseModel Model, object ListEntry, EventServices EventServices)
    {
        public OnBeforeAddListEntryArgs(IBaseModel model, object listEntry, bool abortAdding, EventServices eventServices) : this(model, listEntry, eventServices) => AbortAdding = abortAdding;
        public bool AbortAdding { get; set; }
    }
    public record OnAfterAddListEntryArgs(IBaseModel Model, object ListEntry, EventServices EventServices);
    public record OnBeforeRemoveListEntryArgs(IBaseModel Model, object ListEntry, EventServices EventServices)
    {
        public OnBeforeRemoveListEntryArgs(IBaseModel model, object listEntry, bool abortRemoving, EventServices eventServices) : this(model, listEntry, eventServices) => AbortRemoving = abortRemoving;
        public bool AbortRemoving { get; set; }
    }
    public record OnAfterRemoveListEntryArgs(IBaseModel Model, object ListEntry, EventServices EventServices);
    public record OnAfterMoveListEntryUpArgs(IBaseModel Model, object ListEntry, EventServices EventServices);
    public record OnAfterMoveListEntryDownArgs(IBaseModel Model, object ListEntry, EventServices EventServices);
    #endregion
}
