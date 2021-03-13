using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Models
{
    public class BaseModel
    {
        public event EventHandler<string> OnForcePropertyRepaint;

        public BaseModel()
        {
            CreatedOn = DateTime.Now;
            ModifiedOn = CreatedOn;
        }

        #region Additional Properties
        [ReadOnlyInGUI]
        [Display(Name = "Erstellt am")]
        public DateTime CreatedOn { get; set; }

        [ReadOnlyInGUI]
        [Display(Name = "Geändert am")]
        public DateTime ModifiedOn { get; set; }
        #endregion

        #region [] Extension Methods
        [HideInGUI]
        public object this[PropertyInfo property]
        {
            get
            {
                return property.GetValue(this);
            }
            set
            {
                property.SetValue(this, value);
            }
        }

        [HideInGUI]
        public object this[string property]
        {
            get
            {
                return GetType().GetProperty(property).GetValue(this);
            }
            set
            {
                GetType().GetProperty(property).SetValue(this, value);
            }
        }
        #endregion

        #region Attribute Methods
        public List<PropertyInfo> GetVisibleProperties()
        {
            return GetType().GetVisibleProperties();
        }

        public string GetDisplayName(string propertyName)
        {
            return GetType().GetProperty(propertyName).GetDisplayName();
        }
        #endregion

        #region CRUD Methods

        public virtual void ForcePropertyRepaint(string propertyName)
        {
            OnForcePropertyRepaint?.Invoke(this, propertyName);
        }

        public virtual async Task OnBeforePropertyChanged(PropertyInfo property, object newValue)
        {
        }

        public virtual async Task OnAfterPropertyChanged(PropertyInfo property)
        {
        }

        public virtual async Task<bool> OnBeforeAddEntry()
        {

            return await Task.FromResult(true);
        }

        public virtual async Task OnAfterAddEntry()
        {
        }

        public virtual async Task<bool> OnBeforeUpdateEntry()
        {
            return await Task.FromResult(true);
        }

        public virtual async Task OnAfterUpdateEntry()
        {
        }
        #endregion

        #region Validation Methods
        private ValidationContext ObjectValidationContextInstance;
        [HideInGUI]
        public ValidationContext ObjectValidationContext
        {
            get
            {
                if (ObjectValidationContextInstance == null)
                    ObjectValidationContextInstance = new ValidationContext(this, serviceProvider: null, items: null);

                return ObjectValidationContextInstance;
            }
        }

        public bool TryValidate(out List<ValidationResult> validationResults)
        {
            validationResults = new List<ValidationResult>();
            return Validator.TryValidateObject(this, ObjectValidationContext, validationResults);
        }

        public bool TryValidateProperty(out List<ValidationResult> validationResults, ValidationContext propertyValidationContext, PropertyInfo propertyInfo)
        {
            validationResults = new List<ValidationResult>();
            var attributes = propertyInfo.GetCustomAttributes<Attribute>().OfType<ValidationAttribute>();
            foreach (var attribute in attributes.Where(attribute => attribute.ErrorMessage == null))
            {
                var attributeName = attribute.GetType().Name;
                if (ValidationAttributesTranslations.ResourceManager.GetString(attributeName) != null)
                {
                    attribute.ErrorMessageResourceType = typeof(ValidationAttributesTranslations);
                    attribute.ErrorMessageResourceName = attributeName;
                }
            }

            return Validator.TryValidateValue(propertyInfo.GetValue(this), propertyValidationContext, validationResults, attributes);
        }
        #endregion

    }
}
