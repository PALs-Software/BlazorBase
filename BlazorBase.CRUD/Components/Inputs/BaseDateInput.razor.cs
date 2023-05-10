#nullable enable

using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Timers;

namespace BlazorBase.CRUD.Components.Inputs;

public partial class BaseDateInput
{
    #region Parameter
    [Parameter] public EventCallback<ChangeEventArgs> OnInput { get; set; }
    [Parameter] public DateTime? Value { get; set; }
    [Parameter] public int InputDelay { get; set; } = 200;
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalInputAttributes { get; set; }
    #endregion

    #region Member
    protected Timer Timer = null!;
    protected ChangeEventArgs LastChangeEventArgs = null!;
    protected string? CurrentValueAsString { get; set; }
    #endregion

    protected override void OnInitialized()
    {
        Timer = new Timer(InputDelay);
        Timer.Elapsed += OnSendOnInputEvent;
        Timer.AutoReset = false;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        CurrentValueAsString = Value?.ToString("yyyy-MM-dd");
    }

    protected void DelayOnInputEvent(ChangeEventArgs args)
    {
        Timer.Stop();
        CurrentValueAsString = (string?)args.Value;
        args.Value = ParseStringToDateTime(args.Value);
        LastChangeEventArgs = args;
        Timer.Start();
    }

    protected DateTime? ParseStringToDateTime(object? value)
    {
        if (DateTime.TryParse((string?)value, out DateTime result))
            return result;
        else
            return null;
    }

    protected void OnSendOnInputEvent(object? sender, ElapsedEventArgs e)
    {
        InvokeAsync(() => OnInput.InvokeAsync(LastChangeEventArgs));
    }
}
