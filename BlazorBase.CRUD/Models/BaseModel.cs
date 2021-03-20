using BlazorBase.CRUD.Components;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Resources.ValidationAttributes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Models
{
    public class BaseModel<TModel> : ComponentBase where TModel : class, IBaseModel, new()
    {
        public event EventHandler<string> OnForcePropertyRepaint;

        public BaseModel()
        {
            CreatedOn = DateTime.Now;
            ModifiedOn = CreatedOn;
        }

        #region Additional Properties
        [Display]
        [Editable(false)]
        public DateTime CreatedOn { get; set; }

        [Display]
        [Editable(false)]
        public DateTime ModifiedOn { get; set; }
        #endregion

        #region [] Extension Methods
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

        public object[] GetPrimaryKeys()
        {
            var keyProperties = GetType().GetProperties().Where(property =>
                (!property.PropertyType.IsSubclassOf(typeof(IBaseModel))) &&
                property.IsKey()
            ).ToList();

            var keys = new object[keyProperties.Count];
            for (int i = 0; i < keyProperties.Count; i++)
                keys[i] = keyProperties.ElementAt(i).GetValue(this);

            return keys;
        }

        public string GetPrimaryKeysAsString()
        {
            return String.Join(", ", GetPrimaryKeys());
        }
        #endregion

        #region CRUD Methods

        public void ForcePropertyRepaint(string propertyName)
        {
            OnForcePropertyRepaint?.Invoke(this, propertyName);
        }

        #region Events
        public virtual async Task OnBeforePropertyChanged(PropertyInfo property, object newValue)
        {
            await Task.Run(() => Task.CompletedTask);
        }
        public virtual async Task OnAfterPropertyChanged(PropertyInfo property)
        {
            await Task.Run(() => Task.CompletedTask);
        }

        public virtual async Task<bool> OnBeforeAddEntry(DbContext dbContext)
        {
            return await Task.FromResult(true);
        }

        public virtual async Task OnAfterAddEntry(DbContext dbContext)
        {
            await Task.Run(() => Task.CompletedTask);
        }

        public virtual async Task<bool> OnBeforeUpdateEntry(DbContext dbContext)
        {
            return await Task.FromResult(true);
        }

        public virtual async Task OnAfterUpdateEntry(DbContext dbContext)
        {
            await Task.Run(() => Task.CompletedTask);
        }
        #endregion

        #endregion


        #region Validation Methods
        private ValidationContext ObjectValidationContextInstance;
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

        #region ComponentBase
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            builder.OpenComponent<BaseList<TModel>>(0);
            builder.CloseComponent();
        }
        #endregion
    }
}
