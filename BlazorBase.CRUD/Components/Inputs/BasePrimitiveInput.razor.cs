using BlazorBase.Abstractions.CRUD.Enums;
using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.Abstractions.General.Extensions;
using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Services;
using BlazorBase.Services;
using Blazorise.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static BlazorBase.Abstractions.CRUD.Arguments.BasePrimitiveInputArgs;

namespace BlazorBase.CRUD.Components.Inputs;

public partial class BasePrimitiveInput
{
    #region Parameters
    [Parameter] public object? Value { get; set; } = null!;
    [Parameter] public Type Type { get; set; } = null!;
    [Parameter] public bool ReadOnly { get; set; }

    [Parameter] public string? PlaceHolder { get; set; } = null;
    [Parameter] public int? MinValue { get; set; } = null;
    [Parameter] public int? MaxValue { get; set; } = null;
    [Parameter] public PresentationDataType? PresentationDataTypeParameter { get; set; } = null;
    [Parameter] public PresentationRulesAttribute? PresentationRules { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalInputAttributes { get; set; }

    #region Events
   
    [Parameter] public EventCallback<OnFormatValueArgs> OnFormatValue { get; set; }
    [Parameter] public EventCallback<OnBeforeConvertTypeArgs> OnBeforeConvertType { get; set; }
    [Parameter] public EventCallback<OnValueChangedArgs> OnValueChanged { get; set; }
    
    #endregion

    #endregion

    #region Injects
    [Inject] protected BaseParser BaseParser { get; set; } = null!;
    [Inject] protected BaseErrorHandler ErrorHandler { get; set; } = null!;
    #endregion

    #region Members
    protected string? InputClass;
    protected string? FeedbackClass;
    protected string? Feedback;


    protected bool LastValueConversionFailed = false;

    protected ValidationContext PropertyValidationContext = null!;

    protected Dictionary<string, object> InputAttributes = [];
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
        ArgumentNullException.ThrowIfNull(Type, nameof(Type));


        if (PresentationDataTypeParameter == PresentationDataType.Password && Type == typeof(string))
            IsPasswordInput = true;

        SetInputType();
        SetInputAttributes();

        var currentValue = Value;
        if (IsPasswordInput && currentValue is string currentValueAsString && !String.IsNullOrEmpty(currentValueAsString))
            currentValue = PasswordPlaceholder;

        SetCurrentValueAsString(currentValue);

        await ValidatePropertyValueAsync();
        await RaiseOnFormatPropertyEventsAsync();
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);

        SetInputAttributes();

        var currentValue = Value;
        if (IsPasswordInput && currentValue is string currentValueAsString && !String.IsNullOrEmpty(currentValueAsString))
            currentValue = PasswordViewEnabled ? currentValueAsString.DecryptStringToInsecureString() : PasswordPlaceholder;

        SetCurrentValueAsString(currentValue);

        await RaiseOnFormatPropertyEventsAsync();
    }
    #endregion

    #region Events        

    protected virtual bool ConvertValueIfNeeded(ref object? newValue, Type converType, bool doOnlyConversion = false)
    {
        if (newValue == null || newValue.GetType() == converType || converType == typeof(object) || (newValue is IBaseModel baseModel && baseModel.GetUnproxiedType() == Type))
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
        if (ReadOnly)
            return;

        try
        {
            if (newValue is ChangeEventArgs changeEventArgs)
                newValue = changeEventArgs.Value;

            var oldValue = Value;
            var convertArgs = new OnBeforeConvertTypeArgs(newValue, oldValue);
            await OnBeforeConvertType.InvokeAsync(convertArgs);
            newValue = convertArgs.NewValue;

            LastValueConversionFailed = !ConvertValueIfNeeded(ref newValue, Type);
            if (LastValueConversionFailed)
            {
                await RaiseOnFormatPropertyEventsAsync();
                return;
            }

            if (IsPasswordInput && newValue is string newValueAsString && !String.IsNullOrEmpty(newValueAsString))
                newValue = newValueAsString.EncryptString();

            var valid = await ValidatePropertyValueAsync(calledFromOnValueChangedAsync: true);

            var onValueChangedArgs = new OnValueChangedArgs(newValue, oldValue, valid);
            await OnValueChanged.InvokeAsync(onValueChangedArgs);
            newValue = onValueChangedArgs.NewValue;

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

    #endregion

    #region Validation

    public virtual Task<bool> ValidatePropertyValueAsync(bool calledFromOnValueChangedAsync = false)
    {
        if (LastValueConversionFailed)
            return Task.FromResult(false);

        bool isValid = true;
        string? errorMessage = null;
        var onBeforeArgs = new OnBeforeValidateArgs();
        if (onBeforeArgs.IsHandled)
        {
            isValid = onBeforeArgs.IsValid;
            errorMessage = onBeforeArgs.ErrorMessage;
        }

        var onAfterArgs = new OnAfterValidateArgs(isValid, errorMessage);
        isValid = onAfterArgs.IsValid;
        errorMessage = onAfterArgs.ErrorMessage;

        if (isValid)
            SetValidation(showValidation: false);
        else
            SetValidation(feedback: errorMessage);

        return Task.FromResult(isValid);
    }

    public void SetValidation(bool showValidation = true, bool isValid = false, string? feedback = null)
    {
        FeedbackClass = isValid ? "valid-feedback" : "invalid-feedback";
        if (!showValidation)
        {
            InputClass = String.Empty;
            Feedback = String.Empty;
            return;
        }

        Feedback = feedback;
        InputClass = isValid ? "is-valid" : "is-invalid";
    }

    #endregion

    #region Other

    protected async Task RaiseOnFormatPropertyEventsAsync()
    {
        var formatArgs = new OnFormatValueArgs(InputAttributes, FeedbackClass, InputClass, Feedback, ReadOnly);
        await OnFormatValue.InvokeAsync(formatArgs);

        FeedbackClass = formatArgs.FeedbackClass;
        InputClass = formatArgs.InputClass;
        Feedback = formatArgs.Feedback;
        ReadOnly = formatArgs.IsReadOnly;
    }

    #endregion

    #region RawInputs
    protected void SetInputType()
    {
        if (Type == typeof(string))
            InputType = GetInputTypeByDataType(PresentationDataTypeParameter);
        else if (BaseParser.DecimalTypes.Contains(Type))
            InputType = "number";
    }

    protected string GetInputTypeByDataType(PresentationDataType? dataType)
    {
        if (dataType == null)
            return "text";

        switch (dataType)
        {
            case PresentationDataType.Password:
                return "password";

            case PresentationDataType.EmailAddress:
                return "email";

            case PresentationDataType.Url:
                return "url";

            case PresentationDataType.Color:
                return "color";

            default:
                return "text";
        }
    }

    protected void SetInputAttributes()
    {
        InputAttributes.Clear();

        if (PlaceHolder != null)
            InputAttributes.Add("placeholder", PlaceHolder);

        if (Type != typeof(string) && !BaseParser.DecimalTypes.Contains(Type))
            return;

        SetTextInputAttributes();
        SetDecimalInputAttributes();
    }

    protected void SetTextInputAttributes()
    {
        if (ReadOnly)
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
        if (MinValue != null)
            InputAttributes.Add("min", MinValue);
        if (MaxValue != null)
            InputAttributes.Add("max", MaxValue);

        if (Type != typeof(string))
            InputAttributes.Add("lang", CultureInfo.CurrentUICulture.Name);

        if (Type == typeof(decimal) || Type == typeof(decimal?) ||
            Type == typeof(double) || Type == typeof(double?) ||
            Type == typeof(float) || Type == typeof(float?))
            InputAttributes.Add("step", "any");
    }

    protected virtual void SetCurrentValueAsString(object? input)
    {
        if (Type != typeof(string) && !BaseParser.DecimalTypes.Contains(Type))
            return;

        CurrentValueAsString = FormatValueAsString(input);

        RefreshPresentationRules();
    }

    protected virtual string? FormatValueAsString(object? value)
    {
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

        if (String.IsNullOrEmpty(Value?.ToString()))
            return;

        CurrentValueAsString = PasswordPlaceholder;
    }

    protected void OnViewPasswordClicked()
    {
        PasswordViewEnabled = !PasswordViewEnabled;

        if (PasswordViewEnabled)
        {
            InputType = "text";
            CurrentValueAsString = (Value as string)?.DecryptStringToInsecureString();
        }
        else
        {
            InputType = "password";
            CurrentValueAsString = PasswordPlaceholder;
        }
    }

    #endregion

}
