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

        public Task OnBeforePropertyChanged(PropertyInfo property, object newValue);

        public Task OnAfterPropertyChanged(PropertyInfo property);

        public Task<bool> OnBeforeAddEntry(DbContext dbContext);

        public Task OnAfterAddEntry(DbContext dbContext);

        public Task<bool> OnBeforeUpdateEntry(DbContext dbContext);

        public Task OnAfterUpdateEntry(DbContext dbContext);
        #endregion

        #region Validation Methods
        public bool TryValidate(out List<ValidationResult> validationResults);
        public bool TryValidateProperty(out List<ValidationResult> validationResults, ValidationContext propertyValidationContext, PropertyInfo propertyInfo);
        #endregion
    }
}
