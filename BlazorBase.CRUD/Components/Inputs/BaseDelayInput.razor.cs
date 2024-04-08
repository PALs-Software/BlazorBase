using BlazorBase.CRUD.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Timers;

namespace BlazorBase.CRUD.Components.Inputs;

public partial class BaseDelayInput<TValue> : ComponentBase
{
    #region Parameter
    [Parameter] public EventCallback<ChangeEventArgs> OnDelayedInput { get; set; }
    [Parameter] public TValue? Value { get; set; }
    [Parameter] public EventCallback<TValue?> ValueChanged { get; set; }
    [Parameter] public Type? DeviatingConvertType { get; set; }
    [Parameter] public string? InputType { get; set; }
    [Parameter] public int InputDelay { get; set; } = 250;
    [Parameter] public string? CultureName { get; set; } = CultureInfo.CurrentUICulture.Name;
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalInputAttributes { get; set; } = [];
    #endregion

    #region Injects
    [Inject] protected BaseParser BaseParser { get; set; } = null!;
    #endregion

    #region Member
    protected Timer? Timer;
    protected ChangeEventArgs? LastChangeEventArgs;
    protected string? InternalValue;
    #endregion

    protected override void OnInitialized()
    {
        Timer = new Timer(InputDelay);
        Timer.Elapsed += OnSendDelayedInputEvent;
        Timer.AutoReset = false;

        if (InputType == null)
        {
            if (DeviatingConvertType == null ? BaseParser.DecimalTypes.Contains(typeof(TValue)) : BaseParser.DecimalTypes.Contains(DeviatingConvertType))
                InputType = "number";
            else
                InputType = "text";
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (Timer != null && Timer.Interval != InputDelay) // Setting the Interval will restart the timer and OnSendDelayedInputEvent will be called endless, this check is there to prevent that
            Timer.Interval = InputDelay;

        InternalValue = Value?.ToString();
    }

    protected async Task DelayOnInputEventAsync(ChangeEventArgs args)
    {        
        var newValue = args.Value;
        ConvertValueIfNeeded(ref newValue, DeviatingConvertType == null ? typeof(TValue) : DeviatingConvertType);

        Value = (TValue?)newValue;
        await ValueChanged.InvokeAsync(Value);

        Timer?.Stop();
        LastChangeEventArgs = args;
        Timer?.Start();
    }

    protected void OnSendDelayedInputEvent(object? sender, ElapsedEventArgs e)
    {
        InvokeAsync(() => OnDelayedInput.InvokeAsync(LastChangeEventArgs));
    }

    #region MISC

    protected virtual void ConvertValueIfNeeded(ref object? newValue, Type targetType)
    {
        if (newValue == null || newValue.GetType() == targetType)
            return;

        if (BaseParser.TryParseValueFromString(targetType, newValue.ToString(), out object? parsedValue, out _))
            newValue = parsedValue;
        else
            newValue = null;
    }

    #endregion
}
