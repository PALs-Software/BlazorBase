using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.Services;
using Blazorise.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Components.BaseDisplayComponent;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseInput
    {
        #region Parameters
        [Parameter] public IBaseModel Model { get; set; }
        [Parameter] public PropertyInfo Property { get; set; }
        [Parameter] public bool? ReadOnly { get; set; }
        [Parameter] public BaseService Service { get; set; }
        [Parameter] public IStringLocalizer ModelLocalizer { get; set; }
        [Parameter] public DisplayItem DisplayItem { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> AdditionalInputAttributes { get; set; }
        #region Events
        [Parameter] public EventCallback<OnFormatPropertyArgs> OnFormatProperty { get; set; }
        [Parameter] public EventCallback<OnBeforeConvertPropertyTypeArgs> OnBeforeConvertPropertyType { get; set; }
        [Parameter] public EventCallback<OnBeforePropertyChangedArgs> OnBeforePropertyChanged { get; set; }
        [Parameter] public EventCallback<OnAfterPropertyChangedArgs> OnAfterPropertyChanged { get; set; }

        #endregion

        #endregion

        #region Injects        
        [Inject] protected BaseParser BaseParser { get; set; }
        [Inject] protected IServiceProvider ServiceProvider { get; set; }
        [Inject] protected IMessageHandler MessageHandler { get; set; }
        [Inject] protected BaseErrorHandler ErrorHandler { get; set; }
        #endregion

        #region Members
        protected string InputClass;
        protected string FeedbackClass;
        protected string Feedback;
        protected bool IsReadOnly;
        protected Type RenderType;
        protected DataType? PresentationDataType = null;
        protected string CustomPropertyCssStyle;
        protected bool LastValueConversionFailed = false;

        protected ValidationContext PropertyValidationContext;
        protected PropertyInfo ForeignKeyProperty { get; set; }
        protected PresentationRulesAttribute PresentationRules { get; set; }

        protected Dictionary<string, object> InputAttributes = new Dictionary<string, object>();
        protected string CurrentValueAsString;
        protected string InputType;
        #endregion

        #region Init

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                Model.OnForcePropertyRepaint += Model_OnForcePropertyRepaint;

                if (ReadOnly == null)
                    IsReadOnly = Property.IsReadOnlyInGUI();
                else
                    IsReadOnly = ReadOnly.Value;

                RenderType = Property.GetCustomAttribute<RenderTypeAttribute>()?.RenderType ?? Property.PropertyType;
                CustomPropertyCssStyle = Property.GetCustomAttribute<CustomPropertyCssStyleAttribute>()?.Style;
                PresentationDataType = Property.GetCustomAttribute<DataTypeAttribute>()?.DataType;

                var dict = new Dictionary<object, object>()
                {
                    [typeof(IStringLocalizer)] = ModelLocalizer,
                    [typeof(BaseService)] = Service
                };

                var propertyNameTranslation = ModelLocalizer[Property.Name].ToString();
                if (String.IsNullOrEmpty(propertyNameTranslation))
                    propertyNameTranslation = Property.Name;

                PropertyValidationContext = new ValidationContext(Model, ServiceProvider, dict)
                {
                    MemberName = Property.Name,
                    DisplayName = propertyNameTranslation
                };

                if (Property.TryGetAttribute(out ForeignKeyAttribute foreignKey))
                    ForeignKeyProperty = Model.GetUnproxiedType().GetProperties().Where(entry => entry.Name == foreignKey.Name).FirstOrDefault();

                if (Property.TryGetAttribute(out PresentationRulesAttribute presentationRules))
                    PresentationRules = presentationRules;

                SetInputType();
                SetInputAttributes();
                SetCurrentValueAsString(Property.GetValue(Model));

                ValidatePropertyValue();
            });

            await RaiseOnFormatPropertyEventsAsync();
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);

            if (Property == null || Model == null)
                return;

            if (ReadOnly == null)
                IsReadOnly = Property.IsReadOnlyInGUI();
            else
                IsReadOnly = ReadOnly.Value;

            SetInputAttributes();
            SetCurrentValueAsString(Property.GetValue(Model));
            await RaiseOnFormatPropertyEventsAsync();
        }
        #endregion

        #region Events        
        private void Model_OnForcePropertyRepaint(object sender, string propertyName)
        {
            if (propertyName != Property.Name)
                return;

            LastValueConversionFailed = false;
            SetCurrentValueAsString(Property.GetValue(Model));

            ValidatePropertyValue();

            InvokeAsync(async () =>
            {
                await RaiseOnFormatPropertyEventsAsync();
                StateHasChanged();
            });
        }

        protected virtual bool ConvertValueIfNeeded(ref object newValue, Type converType)
        {
            if (newValue == null || newValue.GetType() == converType)
                return true;

            if (newValue is decimal decimalNewValue)
                newValue = decimalNewValue.ToString(CultureInfo.InvariantCulture);
            else if (newValue is double doubleNewValue)
                newValue = doubleNewValue.ToString(CultureInfo.InvariantCulture);
            else if (newValue is float floatNewValue)
                newValue = floatNewValue.ToString(CultureInfo.InvariantCulture);
            else if (newValue is DateTimeOffset dateTimeOffsetNewvalue)
                newValue = dateTimeOffsetNewvalue.ToString(CultureInfo.InvariantCulture);
            else if (newValue is DateTime dateTimeNewvalue)
                newValue = dateTimeNewvalue.ToString(CultureInfo.InvariantCulture);

            if (BaseParser.TryParseValueFromString(converType, newValue.ToString(), out object parsedValue, out string errorMessage))
            {
                newValue = parsedValue;
                return true;
            }

            SetCurrentValueAsString(newValue);
            SetValidation(feedback: errorMessage);
            return false;
        }

        protected async virtual Task OnValueChangedAsync(object newValue)
        {
            if (IsReadOnly)
                return;

            try
            {
                if (newValue is ChangeEventArgs changeEventArgs)
                    newValue = changeEventArgs.Value;

                var oldValue = Property.GetValue(Model);
                var eventServices = GetEventServices();
                var convertArgs = new OnBeforeConvertPropertyTypeArgs(Model, Property.Name, newValue, oldValue, eventServices);
                await OnBeforeConvertPropertyType.InvokeAsync(convertArgs);
                await Model.OnBeforeConvertPropertyType(convertArgs);
                newValue = convertArgs.NewValue;

                LastValueConversionFailed = !ConvertValueIfNeeded(ref newValue, RenderType);
                if (RenderType != Property.PropertyType)
                {
                    LastValueConversionFailed = !ConvertValueIfNeeded(ref newValue, Property.PropertyType);
                    if (!LastValueConversionFailed && BaseParser.DecimalTypes.Contains(Property.PropertyType))
                        if (newValue != null && (RenderType == typeof(int) || RenderType == typeof(int?) || RenderType == typeof(long) || RenderType == typeof(long?)))
                            newValue = Convert.ChangeType(Convert.ChangeType(newValue, RenderType), Property.PropertyType); //Round numeric values if rendertype is without decimal places and the property type has decimal places
                }

                if (LastValueConversionFailed)
                {
                    await RaiseOnFormatPropertyEventsAsync();
                    return;
                }

                var args = new OnBeforePropertyChangedArgs(Model, Property.Name, newValue, oldValue, eventServices);
                await OnBeforePropertyChanged.InvokeAsync(args);
                await Model.OnBeforePropertyChanged(args);
                newValue = args.NewValue;

                Property.SetValue(Model, newValue);
                var valid = ValidatePropertyValue();

                if (valid)
                    await ReloadForeignProperties(newValue);

                var onAfterArgs = new OnAfterPropertyChangedArgs(Model, Property.Name, newValue, oldValue, valid, eventServices);
                await OnAfterPropertyChanged.InvokeAsync(onAfterArgs);
                await Model.OnAfterPropertyChanged(onAfterArgs);

                SetCurrentValueAsString(newValue);
            }
            catch (Exception e)
            {
                LastValueConversionFailed = true;
                SetValidation(feedback: ErrorHandler.PrepareExceptionErrorMessage(e));
                await RaiseOnFormatPropertyEventsAsync();
            }
        }

        protected async virtual Task ReloadForeignProperties(object newValue)
        {
            if (ForeignKeyProperty == null || (!typeof(IBaseModel).IsAssignableFrom(ForeignKeyProperty.PropertyType)))
                return;

            if (Service.DbContext.Entry(Model).State == EntityState.Detached)
                ForeignKeyProperty.SetValue(Model, await Service.GetAsync(ForeignKeyProperty.PropertyType, newValue));
            else
                await Service.DbContext.Entry(Model).Reference(ForeignKeyProperty.Name).LoadAsync();
        }
        #endregion

        #region Validation

        public bool ValidatePropertyValue()
        {
            if (LastValueConversionFailed)
                return false;

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

        protected EventServices GetEventServices()
        {
            return new EventServices()
            {
                ServiceProvider = ServiceProvider,
                Localizer = ModelLocalizer,
                BaseService = Service,
                MessageHandler = MessageHandler
            };
        }

        protected async Task RaiseOnFormatPropertyEventsAsync()
        {
            try
            {
                var formatArgs = new OnFormatPropertyArgs(Model, Property.Name, InputAttributes, FeedbackClass, InputClass, Feedback, GetEventServices());
                await OnFormatProperty.InvokeAsync(formatArgs);
                await Model.OnFormatProperty(formatArgs);

                FeedbackClass = formatArgs.FeedbackClass;
                InputClass = formatArgs.InputClass;
                Feedback = formatArgs.Feedback;
            }
            catch (Exception) { }
        }

        #endregion

        #region RawInputs
        protected void SetInputType()
        {
            if (RenderType == typeof(string))
                InputType = GetInputTypeByDataType(PresentationDataType);
            else if (BaseParser.DecimalTypes.Contains(RenderType))
                InputType = "number";
        }

        protected string GetInputTypeByDataType(DataType? dataType)
        {
            if (dataType == null)
                return "text";

            switch (dataType)
            {
                case DataType.EmailAddress:
                    return "email";
                case DataType.Password:
                    return "password";
                case DataType.Url:
                    return "url";
                default:
                    return "text";
            }
        }

        protected void SetInputAttributes()
        {
            InputAttributes.Clear();

            if (!String.IsNullOrEmpty(CustomPropertyCssStyle))
                InputAttributes.Add("style", CustomPropertyCssStyle);

            if (Property.TryGetAttribute(out PlaceholderTextAttribute placeholderAttribute))
                InputAttributes.Add("placeholder", placeholderAttribute.Placeholder);

            if (RenderType != typeof(string) && !BaseParser.DecimalTypes.Contains(RenderType))
                return;

            SetTextInputAttributes();
            SetDecimalInputAttributes();
        }

        protected void SetTextInputAttributes()
        {
            if (IsReadOnly)
                InputAttributes.Add("disabled", "");

            if (AdditionalInputAttributes != null)
                foreach (var item in AdditionalInputAttributes)
                {
                    if (InputAttributes.ContainsKey(item.Key.ToLower()))
                    {
                        if (item.Key.ToLower() == "style")
                            InputAttributes["style"] = $"{InputAttributes["style"]}; {item.Value}";
                        else if (item.Key.ToLower() == "class")
                            InputAttributes["class"] = $"{InputAttributes["class"]} {item.Value}";
                    }
                    else
                        InputAttributes.Add(item.Key, item.Value);
                }
        }

        protected void RefreshPresentationRules()
        {
            if (PresentationRules == null)
                return;

            if (PresentationRules.Rules.Contains(PresentationRule.ShowValueAsTooltip))
                InputAttributes["title"] = CurrentValueAsString;
        }
        protected void SetDecimalInputAttributes()
        {
            if (Property.TryGetAttribute(out RangeAttribute rangeAttribute))
            {
                InputAttributes.Add("min", rangeAttribute.Minimum);
                InputAttributes.Add("max", rangeAttribute.Maximum);
            }

            if (RenderType != typeof(string))
                InputAttributes.Add("lang", CultureInfo.CurrentUICulture.Name);

            if (RenderType == typeof(decimal) || RenderType == typeof(decimal?) ||
                RenderType == typeof(double) || RenderType == typeof(double?) ||
                RenderType == typeof(float) || RenderType == typeof(float?))
                InputAttributes.Add("step", "any");
        }

        protected virtual void SetCurrentValueAsString(object input)
        {
            if (RenderType != typeof(string) && !BaseParser.DecimalTypes.Contains(RenderType))
                return;

            CurrentValueAsString = FormatValueAsString(input);

            RefreshPresentationRules();
        }

        protected virtual string FormatValueAsString(object value)
        {
            try
            {
                if (RenderType != Property.PropertyType && !LastValueConversionFailed)
                    value = Convert.ChangeType(value, RenderType);
            }
            catch (Exception) { }

            if (value is null)
                return null;
            else if (value is string @string)
                return @string;
            else if (value is int @int)
                return Converters.FormatValue(@int);
            else if (value is long @long)
                return Converters.FormatValue(@long);
            else if (value is decimal @decimal)
                return @decimal.ToString("0.00##", CultureInfo.InvariantCulture);
            else if (value is double @double)
                return @double.ToString("0.00##", CultureInfo.InvariantCulture);
            else if (value is float @float)
                return @float.ToString("0.00##", CultureInfo.InvariantCulture);
            else
                throw new InvalidOperationException($"Unsupported type {value.GetType()}");
        }
        #endregion

    }
}
