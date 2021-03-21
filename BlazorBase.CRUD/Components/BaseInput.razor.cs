using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Modules;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseInput<TModel> where TModel : IBaseModel
    {
        [Parameter]
        public TModel Model { get; set; }

        [Parameter]
        public PropertyInfo Property { get; set; }

        [Parameter]
        public bool ReadOnly { get; set; }

        [Parameter]
        public BaseService Service { get; set; }

        [Inject]
        private IStringLocalizer<TModel> ModelLocalizer { get; set; }

        [Inject]
        private IServiceProvider ServiceProvider { get; set; }

        protected string ValidationClass;
        protected string ValidFeedback;
        protected string InvalidFeedback;
        protected string InputType;
        protected string CurrentValueAsString;
        protected virtual bool UseGenericNullString { get; set; } = false;
        protected Dictionary<string, object> InputAttributes = new Dictionary<string, object>();
        protected ValidationContext PropertyValidationContext;


        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                var nullString = UseGenericNullString ? BaseConstants.GenericNullString : String.Empty;
                CurrentValueAsString = Property.GetValue(Model)?.ToString() ?? nullString;
                Model.OnForcePropertyRepaint += Model_OnForcePropertyRepaint;
                SetInputAttributes();

                InputType = GetInputType();

                var dict = new Dictionary<object, object>()
                {
                    [typeof(IStringLocalizer<TModel>)] = ModelLocalizer,
                    [typeof(DbContext)] = Service.DbContext
                };

                PropertyValidationContext = new ValidationContext(Model, ServiceProvider, dict)
                {
                    MemberName = Property.Name
                };

                ValidatePropertyValue();
            });
        }

        private void Model_OnForcePropertyRepaint(object sender, string propertyName)
        {
            if (propertyName != Property.Name)
                return;

            var nullString = UseGenericNullString ? BaseConstants.GenericNullString : String.Empty;
            CurrentValueAsString = Property.GetValue(Model)?.ToString() ?? nullString;
            ValidatePropertyValue();
            StateHasChanged();
        }

        public object GetCurrentPropertyValue()
        {
            return Property.GetValue(Model);
        }

        protected async void OnValueChanged(ChangeEventArgs e)
        {
            CurrentValueAsString = e.Value.ToString();
            var newValue = CurrentValueAsString;
            if (UseGenericNullString && newValue == BaseConstants.GenericNullString)
                newValue = null;

            await Model.OnBeforePropertyChanged(Property.Name, ref newValue, GetEventServices());
         
            if (BaseParser.TryParseValueFromString(Property.PropertyType, newValue, out object parsedValue, out string errorMessage))
            {
                Property.SetValue(Model, parsedValue);
                var valid = ValidatePropertyValue();

                await Model.OnAfterPropertyChanged(Property.Name, parsedValue, valid, GetEventServices());
            }
            else
                SetValidation(feedback: errorMessage);
        }

        protected string GetInputType()
        {
            var type = Property.PropertyType;
            if (type == typeof(String))
                return "text";
            else if (type == typeof(int))
                return "number";
            else if (type == typeof(decimal))
                return "number";
            else if (type == typeof(long))
                return "number";
            else if (type == typeof(bool))
                return "checkbox";
            else if (type == typeof(DateTime))
                return "datetime";
            if (type == typeof(Guid))
                return "text";
            else
                throw new Exception("Type not supported");
        }

        public bool ValidatePropertyValue()
        {
            if (Model.TryValidateProperty(out List<ValidationResult> validationResults, PropertyValidationContext, Property))
            {
                SetValidation(showValidation: false);
                return true;
            }

            SetValidation(feedback: String.Join(", ", validationResults.Select(results => results.ErrorMessage)));
            return false;
        }

        public void SetValidation(bool showValidation = true, bool isValid = false, string feedback = "")
        {
            if (!showValidation)
            {
                ValidationClass = String.Empty;
                return;
            }

            ValidFeedback = feedback;
            InvalidFeedback = feedback;
            ValidationClass = isValid ? "is-valid" : "is-invalid";
        }

        private void SetInputAttributes()
        {
            if (ReadOnly || Property.IsReadOnlyInGUI())
                InputAttributes.Add("disabled", "");

            if (Property.TryGetAttribute(out RangeAttribute rangeAttribute))
            {
                InputAttributes.Add("min", rangeAttribute.Minimum);
                InputAttributes.Add("max", rangeAttribute.Maximum);
            }

            if (Property.TryGetAttribute(out PlaceholderTextAttribute placeholderAttribute))
                InputAttributes.Add("placeholder", placeholderAttribute.Placeholder);
        }

        private EventServices GetEventServices()
        {
            return new EventServices()
            {
                ServiceProvider = ServiceProvider,
                Localizer = ModelLocalizer,
                DbContext = Service.DbContext                
            };
        }
    }
}
