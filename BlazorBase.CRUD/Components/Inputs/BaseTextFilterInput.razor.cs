using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Timers;

namespace BlazorBase.CRUD.Components.Inputs;

public partial class BaseTextFilterInput
{
    #region Parameter
    [Parameter] public EventCallback<ChangeEventArgs> OnInput { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public int InputDelay { get; set; } = 200;
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalInputAttributes { get; set; } = new();
    #endregion

    #region Member
    protected Timer? Timer;
    protected ChangeEventArgs? LastChangeEventArgs;
    #endregion

    protected override void OnInitialized()
    {
        Timer = new Timer(InputDelay);
        Timer.Elapsed += OnSendOnInputEvent;
        Timer.AutoReset = false;
    }


    protected void DelayOnInputEvent(ChangeEventArgs args)
    {
        Timer?.Stop();
        LastChangeEventArgs = args;
        Timer?.Start();
    }

    protected void OnSendOnInputEvent(object? sender, ElapsedEventArgs e)
    {
        InvokeAsync(() => OnInput.InvokeAsync(LastChangeEventArgs));
    }
}
