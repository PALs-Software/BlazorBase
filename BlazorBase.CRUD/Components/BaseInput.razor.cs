using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using Blazorise;
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
using static BlazorBase.CRUD.Models.IBaseModel;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseInput
    {
        #region Parameters
        [Parameter] public IBaseModel Model { get; set; }
        [Parameter] public PropertyInfo Property { get; set; }
        [Parameter] public bool ReadOnly { get; set; }
        [Parameter] public BaseService Service { get; set; }
        [Parameter] public IStringLocalizer ModelLocalizer { get; set; }

        #region Events
        [Parameter] public EventCallback<OnBeforeConvertPropertyTypeArgs> OnBeforeConvertPropertyType { get; set; }
        [Parameter] public EventCallback<OnBeforePropertyChangedArgs> OnBeforePropertyChanged { get; set; }
        [Parameter] public EventCallback<OnAfterPropertyChangedArgs> OnAfterPropertyChanged { get; set; }

        #endregion

        #endregion

        #region Injects
        [Inject] protected BaseParser BaseParser { get; set; }
        [Inject] protected IServiceProvider ServiceProvider { get; set; }
        #endregion

        #region Members
        protected string InputClass;
        protected string FeedbackClass;
        protected string Feedback;
        protected string PlaceHolder;
        protected bool IsReadOnly;
        protected Type RenderType;

        protected Dictionary<string, object> InputAttributes = new Dictionary<string, object>();
        protected ValidationContext PropertyValidationContext;

        #endregion

        #region Init

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                Model.OnForcePropertyRepaint += Model_OnForcePropertyRepaint;

                IsReadOnly = ReadOnly || Property.IsReadOnlyInGUI();
                
                if (Property.TryGetAttribute(out PlaceholderTextAttribute placeholderAttribute))
                    PlaceHolder = placeholderAttribute.Placeholder;
                RenderType = Property.GetCustomAttribute<RenderTypeAttribute>()?.RenderType ?? Property.PropertyType; 

                var iStringModelLocalizerType = typeof(IStringLocalizer<>).MakeGenericType(Model.GetUnproxiedType());
                var dict = new Dictionary<object, object>()
                {
                    [iStringModelLocalizerType] = ModelLocalizer,
                    [typeof(BaseService)] = Service
                };

                PropertyValidationContext = new ValidationContext(Model, ServiceProvider, dict)
                {
                    MemberName = Property.Name
                };

                ValidatePropertyValue();
            });
        }
        #endregion

        #region Events

        private void Model_OnForcePropertyRepaint(object sender, string propertyName)
        {
            if (propertyName != Property.Name)
                return;

            ValidatePropertyValue();
            StateHasChanged();
        }

        protected bool ConvertValueIfNeeded(ref object newValue)
        {
            if (newValue == null || newValue.GetType() == RenderType)
                return true;

            if (BaseParser.TryParseValueFromString(RenderType, newValue.ToString(), out object parsedValue, out string errorMessage))
            {
                newValue = parsedValue;
                return true;
            }

            SetValidation(feedback: errorMessage);
            return false;
        }

        protected async Task OnValueChangedAsync(object newValue)
        {
            var eventServices = GetEventServices();
            var convertArgs = new OnBeforeConvertPropertyTypeArgs(Model, Property.Name, newValue, eventServices);
            await OnBeforeConvertPropertyType.InvokeAsync(convertArgs);
            await Model.OnBeforeConvertPropertyType(convertArgs);
            newValue = convertArgs.NewValue;

            if (!ConvertValueIfNeeded(ref newValue))
                return;            

            var args = new OnBeforePropertyChangedArgs(Model, Property.Name, newValue, eventServices);
            await OnBeforePropertyChanged.InvokeAsync(args);
            await Model.OnBeforePropertyChanged(args);
            newValue = args.NewValue;

            Property.SetValue(Model, newValue);
            var valid = ValidatePropertyValue();

            var onAfterArgs = new OnAfterPropertyChangedArgs(Model, Property.Name, newValue, valid, eventServices);
            await OnAfterPropertyChanged.InvokeAsync(onAfterArgs);
            await Model.OnAfterPropertyChanged(onAfterArgs);
        }

        #endregion

        #region Validation

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
            FeedbackClass = isValid ? "valid-feedback" : "invalid-feedback";
            if (!showValidation)
            {
                Feedback = InputClass = String.Empty;
                return;
            }

            Feedback = feedback;
            InputClass = isValid ? "is-valid" : "is-invalid";
        }

        #endregion

        #region Other

        private EventServices GetEventServices()
        {
            return new EventServices()
            {
                ServiceProvider = ServiceProvider,
                Localizer = ModelLocalizer,
                BaseService = Service
            };
        }

        #endregion
    }
}
