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
using Microsoft.AspNetCore.Components.Web;
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
using static BlazorBase.CRUD.Components.General.BaseDisplayComponent;
using static BlazorBase.CRUD.Models.BaseModel;

namespace BlazorBase.CRUD.Components.Inputs;

public partial class BaseInput
{
    #region Parameters
    [Parameter] public IBaseModel Model { get; set; } = null!;
    [Parameter] public PropertyInfo Property { get; set; } = null!;
    [Parameter] public bool? ReadOnly { get; set; }
    [Parameter] public BaseService Service { get; set; } = null!;
    [Parameter] public IStringLocalizer ModelLocalizer { get; set; } = null!;
    [Parameter] public DisplayItem DisplayItem { get; set; } = null!;
    [Parameter] public DataType? InputPresentationDataType { get; set; } = null;
    [Parameter] public List<ValidationAttribute>? AdditionalValidationAttributes { get; set; } = null;
    [Parameter] public ValidationTranslationResource? ValidationTranslationResource { get; set; } = null;
    [Parameter] public bool UserCanViewPasswords { get; set; } = false;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalInputAttributes { get; set; }

    #region Events
    [Parameter] public EventCallback<OnFormatPropertyArgs> OnFormatProperty { get; set; }
    [Parameter] public EventCallback<OnBeforeConvertPropertyTypeArgs> OnBeforeConvertPropertyType { get; set; }
    [Parameter] public EventCallback<OnBeforePropertyChangedArgs> OnBeforePropertyChanged { get; set; }
    [Parameter] public EventCallback<OnAfterPropertyChangedArgs> OnAfterPropertyChanged { get; set; }

    #endregion

    #endregion

    #region Injects        
    [Inject] protected BaseParser BaseParser { get; set; } = null!;
    [Inject] protected IServiceProvider ServiceProvider { get; set; } = null!;
    [Inject] protected IMessageHandler MessageHandler { get; set; } = null!;
    [Inject] protected BaseErrorHandler ErrorHandler { get; set; } = null!;
    #endregion

    #region Members
    protected bool SkipCustomSetParametersAsync = false;
    protected string? InputClass;
    protected string? FeedbackClass;
    protected string? Feedback;
    protected bool IsReadOnly;
    protected Type RenderType = null!;
    protected DataType? PresentationDataType = null;
    protected bool LastValueConversionFailed = false;

    protected ValidationContext PropertyValidationContext = null!;
    protected PropertyInfo? ForeignKeyProperty { get; set; }
    protected PresentationRulesAttribute? PresentationRules { get; set; }

    protected Dictionary<string, object> InputAttributes = new();
    protected string? CurrentValueAsString;
    protected string? InputType;

    #endregion

    #region Password

    protected bool IsPasswordInput;
    protected bool PasswordViewEnabled;
    protected AllowUserPasswordAccessAttribute? AllowUserPasswordAccess;
    public const string PasswordPlaceholder = "&%PASSWORD!PLACEHOLDER&%";

    #endregion

    #region Init

    protected override async Task OnInitializedAsync()
    {
        Model.OnForcePropertyRepaint += Model_OnForcePropertyRepaint;

        if (ReadOnly == null)
            IsReadOnly = Property.IsReadOnlyInGUI();
        else
            IsReadOnly = ReadOnly.Value;

        RenderType = DisplayItem?.DisplayPropertyType ?? Property.PropertyType;
        PresentationDataType = InputPresentationDataType ?? Property.GetCustomAttribute<DataTypeAttribute>()?.DataType;

        var dict = new Dictionary<object, object?>()
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

        if (Property.TryGetAttribute(out ForeignKeyAttribute? foreignKey))
            ForeignKeyProperty = Model.GetUnproxiedType().GetProperties().Where(entry => entry.Name == foreignKey.Name).FirstOrDefault();

        if (Property.TryGetAttribute(out PresentationRulesAttribute? presentationRules))
            PresentationRules = presentationRules;

        if (PresentationDataType == DataType.Password && RenderType == typeof(string))
        {
            IsPasswordInput = true;
            if (Property.TryGetAttribute(out AllowUserPasswordAccessAttribute? allowUserPasswordAccess))
                AllowUserPasswordAccess = await CheckCanViewPasswordAsync(allowUserPasswordAccess) ? allowUserPasswordAccess : null;
        }

        SetInputType();
        SetInputAttributes();

        var currentValue = Property.GetValue(Model);
        if (IsPasswordInput && currentValue is string currentValueAsString && !String.IsNullOrEmpty(currentValueAsString))
            currentValue = PasswordPlaceholder;

        SetCurrentValueAsString(currentValue);

        await ValidatePropertyValueAsync();
        await RaiseOnFormatPropertyEventsAsync();
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);

        if (SkipCustomSetParametersAsync)
            return;

        if (Property == null || Model == null)
            return;

        if (ReadOnly == null)
            IsReadOnly = Property.IsReadOnlyInGUI();
        else
            IsReadOnly = ReadOnly.Value;

        SetInputAttributes();

        var currentValue = Property.GetValue(Model);
        if (IsPasswordInput && currentValue is string currentValueAsString && !String.IsNullOrEmpty(currentValueAsString))
            currentValue = PasswordViewEnabled ? currentValueAsString.DecryptStringToInsecureString() : PasswordPlaceholder;

        SetCurrentValueAsString(currentValue);

        await RaiseOnFormatPropertyEventsAsync();
    }
    #endregion

    #region Events        
    private void Model_OnForcePropertyRepaint(object? sender, string[] propertyNames)
    {
        if (!propertyNames.Contains(Property.Name))
            return;

        LastValueConversionFailed = false;
        SetCurrentValueAsString(Property.GetValue(Model));

        InvokeAsync(async () =>
        {
            await ValidatePropertyValueAsync();
            await RaiseOnFormatPropertyEventsAsync();
            StateHasChanged();
        });
    }

    protected virtual bool ConvertValueIfNeeded(ref object? newValue, Type converType, bool doOnlyConversion = false)
    {
        if (newValue == null || newValue.GetType() == converType || converType == typeof(object) || (newValue is IBaseModel baseModel && baseModel.GetUnproxiedType() == RenderType))
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

        if (BaseParser.TryParseValueFromString(converType, newValue.ToString(), out object? parsedValue, out string? errorMessage))
        {
            newValue = parsedValue;
            return true;
        }

        if (!doOnlyConversion)
        {
            SetCurrentValueAsString(newValue);
            SetValidation(feedback: errorMessage);
        }

        return false;
    }

    protected async virtual Task OnValueChangedAsync(object? newValue, bool setCurrentValueAsString = true)
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

            if (IsPasswordInput && newValue is string newValueAsString && !String.IsNullOrEmpty(newValueAsString))
                newValue = newValueAsString.EncryptString();

            Property.SetValue(Model, newValue);
            var valid = await ValidatePropertyValueAsync();

            if (valid)
                await ReloadForeignProperties(newValue);

            var onAfterArgs = new OnAfterPropertyChangedArgs(Model, Property.Name, newValue, oldValue, valid, eventServices);
            await OnAfterPropertyChanged.InvokeAsync(onAfterArgs);
            await Model.OnAfterPropertyChanged(onAfterArgs);

            if (IsPasswordInput && newValue is string newValueAsString2 && !String.IsNullOrEmpty(newValueAsString2))
                newValue = PasswordViewEnabled ? newValueAsString2.DecryptStringToInsecureString() : PasswordPlaceholder;

            if (setCurrentValueAsString)
                SetCurrentValueAsString(newValue);
        }
        catch (Exception e)
        {
            LastValueConversionFailed = true;
            SetValidation(feedback: ErrorHandler.PrepareExceptionErrorMessage(e));
            await RaiseOnFormatPropertyEventsAsync();
        }
    }

    protected async virtual Task ReloadForeignProperties(object? newValue)
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

    public async Task<bool> ValidatePropertyValueAsync()
    {
        if (LastValueConversionFailed)
            return false;

        bool isValid = true;
        string? errorMessage = null;
        var eventServices = GetEventServices();
        var onBeforeArgs = new OnBeforeValidatePropertyArgs(Model, Property.Name, eventServices);
        await Model.OnBeforeValidateProperty(onBeforeArgs);
        if (onBeforeArgs.IsHandled)
        {
            isValid = onBeforeArgs.IsValid;
            errorMessage = onBeforeArgs.ErrorMessage;
        }
        else
        {
            isValid = Model.TryValidateProperty(out List<ValidationResult> validationResults, PropertyValidationContext, Property, AdditionalValidationAttributes, ValidationTranslationResource);
            errorMessage = String.Join(", ", validationResults.Select(results => results.ErrorMessage));
        }

        var onAfterArgs = new OnAfterValidatePropertyArgs(Model, Property.Name, eventServices, isValid, errorMessage);
        await Model.OnAfterValidateProperty(onAfterArgs);
        isValid = onAfterArgs.IsValid;
        errorMessage = onAfterArgs.ErrorMessage;

        if (isValid)
            SetValidation(showValidation: false);
        else
            SetValidation(feedback: errorMessage);

        return isValid;
    }

    public void SetValidation(bool showValidation = true, bool isValid = false, string? feedback = null)
    {
        FeedbackClass = isValid ? "valid-feedback" : "invalid-feedback";
        if (!showValidation)
        {
            InputClass = DisplayItem?.CustomizationClasses[CustomizationLocation.Input] ?? String.Empty;
            Feedback = String.Empty;
            return;
        }

        Feedback = feedback;
        InputClass = $"{DisplayItem?.CustomizationClasses[CustomizationLocation.Input]} {(isValid ? "is-valid" : "is-invalid")}";
    }

    #endregion

    #region Other

    protected EventServices GetEventServices()
    {
        return new EventServices(ServiceProvider, ModelLocalizer, Service, MessageHandler);
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

        if (DisplayItem != null && !String.IsNullOrEmpty(DisplayItem.CustomizationStyles[CustomizationLocation.Input]))
            InputAttributes.Add("style", DisplayItem.CustomizationStyles[CustomizationLocation.Input]);

        if (Property.TryGetAttribute(out PlaceholderTextAttribute? placeholderAttribute))
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
            InputAttributes["title"] = CurrentValueAsString ?? String.Empty;
    }
    protected void SetDecimalInputAttributes()
    {
        if (Property.TryGetAttribute(out RangeAttribute? rangeAttribute))
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

    protected virtual void SetCurrentValueAsString(object? input)
    {
        if (RenderType != typeof(string) && !BaseParser.DecimalTypes.Contains(RenderType))
            return;

        CurrentValueAsString = FormatValueAsString(input);

        RefreshPresentationRules();
    }

    protected virtual string? FormatValueAsString(object? value)
    {
        if (RenderType != Property.PropertyType && !LastValueConversionFailed)
            ConvertValueIfNeeded(ref value, RenderType, doOnlyConversion: true);

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
        else if (value is Guid @guid)
            return @guid.ToString();
        else
            throw new InvalidOperationException($"Unsupported type {value.GetType()}");
    }
    #endregion

    #region Password

    protected virtual async Task<bool> CheckCanViewPasswordAsync(AllowUserPasswordAccessAttribute allowUserPasswordAccess)
    {
        if (String.IsNullOrEmpty(allowUserPasswordAccess.AllowAccessCallbackMethodName))
            return true;

        var method = Property.ReflectedType?.GetMethod(allowUserPasswordAccess.AllowAccessCallbackMethodName, BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
        var parameters = method?.GetParameters();

        if (method == null ||
            parameters == null ||
            parameters.Length != 4 ||
            parameters[0].ParameterType != typeof(PropertyInfo) ||
            parameters[1].ParameterType != typeof(IBaseModel) ||
            parameters[2].ParameterType != typeof(EventServices) ||
            parameters[3].ParameterType != typeof(bool) ||
            method.ReturnType != typeof(Task<bool>) ||
            !method.IsStatic)
            throw new CRUDException($"The signature of the allow access callback method {allowUserPasswordAccess.AllowAccessCallbackMethodName} in the class {Property.ReflectedType?.Name}, does not match the following signature: public static [async] Task<bool> TheMethodName(PropertyInfo propertyInfo, IBaseModel cardModel, EventServices eventServices, bool readOnly)");

        var result = await (method.Invoke(null, new object?[] { Property, Model, GetEventServices(), ReadOnly }) as Task<bool> ?? Task.FromResult(false));
        return result;
    }

    protected void OnInputFocusIn(FocusEventArgs args)
    {
        if (!IsPasswordInput || PasswordViewEnabled)
            return;

        if (CurrentValueAsString != PasswordPlaceholder)
            return;

        CurrentValueAsString = String.Empty;
    }

    protected void OnInputFocusOut(FocusEventArgs args)
    {
        if (!IsPasswordInput || PasswordViewEnabled)
            return;

        if (CurrentValueAsString != String.Empty)
            return;

        if (String.IsNullOrEmpty((string?)Property.GetValue(Model)))
            return;

        CurrentValueAsString = PasswordPlaceholder;
    }

    protected void OnViewPasswordClicked()
    {
        PasswordViewEnabled = !PasswordViewEnabled;

        if (PasswordViewEnabled)
        {
            InputType = "text";
            var value = Property.GetValue(Model);
            CurrentValueAsString = (value as string)?.DecryptStringToInsecureString();
        }
        else
        {
            InputType = "password";
            CurrentValueAsString = PasswordPlaceholder;
        }
    }

    #endregion

}
