using BlazorBase.CRUD.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Models
{
    public interface IBaseModel
    {
        #region Events
        public event EventHandler<string> OnForcePropertyRepaint;
        #endregion

        #region Attribute Methods
        public List<PropertyInfo> GetVisibleProperties();

        public object[] GetPrimaryKeys();

        public string GetPrimaryKeysAsString();
        #endregion

        #region CRUD Methods
        public void ForcePropertyRepaint(string propertyName);


        public record OnBeforePropertyChangedArgs(IBaseModel Model, string PropertyName, string NewValue, EventServices EventServices);
        public Task OnBeforePropertyChanged(OnBeforePropertyChangedArgs args);


        public record OnAfterPropertyChangedArgs(IBaseModel Model, string PropertyName, object NewValue, bool IsValid, EventServices EventServices);
        public Task OnAfterPropertyChanged(OnAfterPropertyChangedArgs args);


        public record OnBeforeAddEntryArgs(IBaseModel Model, bool AbortAdding, EventServices EventServices);
        public Task OnBeforeAddEntry(OnBeforeAddEntryArgs args);


        public record OnAfterAddEntryArgs(IBaseModel Model, EventServices EventServices);
        public Task OnAfterAddEntry(OnAfterAddEntryArgs args);


        public record OnBeforeUpdateEntryArgs(IBaseModel Model, bool AbortUpdating, EventServices EventServices);
        public Task OnBeforeUpdateEntry(OnBeforeUpdateEntryArgs args);


        public record OnAfterUpdateEntryArgs(IBaseModel Model, EventServices EventServices);
        public Task OnAfterUpdateEntry(OnAfterUpdateEntryArgs args);
        #endregion

        #region Validation Methods
        public bool TryValidate(out List<ValidationResult> validationResults, ValidationContext validationContext);
        public bool TryValidateProperty(out List<ValidationResult> validationResults, ValidationContext propertyValidationContext, PropertyInfo propertyInfo);
        #endregion
    }
}
