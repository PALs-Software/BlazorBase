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

        public Task OnBeforePropertyChanged(string propertyName, ref bool isHandled, ref string newValue, ref InputValidation inputValidation, EventServices eventServices);

        public Task OnAfterPropertyChanged(string propertyName);

        public Task<bool> OnBeforeAddEntry(EventServices eventServices);

        public Task OnAfterAddEntry(EventServices eventServices);

        public Task<bool> OnBeforeUpdateEntry(EventServices eventServices);

        public Task OnAfterUpdateEntry(EventServices eventServices);
        #endregion

        #region Validation Methods
        public bool TryValidate(out List<ValidationResult> validationResults);
        public bool TryValidateProperty(out List<ValidationResult> validationResults, ValidationContext propertyValidationContext, PropertyInfo propertyInfo);
        #endregion
    }
}
