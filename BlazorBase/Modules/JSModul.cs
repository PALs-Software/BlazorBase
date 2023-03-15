using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace BlazorBase.Modules;

public abstract class JSModul : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> ModuleTask;

    /// <summary>
    /// Imports a given javascript modul (with a path like "./_content/BlazorBase/exampleJsInterop.js" into the current session
    /// </summary>
    /// <param name="jsRuntime"></param>
    /// <param name="modulePath"></param>
    public JSModul(IJSRuntime jsRuntime, string modulePath)
    {
        ModuleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath).AsTask());
    }

    protected async ValueTask InvokeVoidAsync(string identifier, params object[]? args) => await (await ModuleTask.Value).InvokeVoidAsync(identifier, args);
    protected async ValueTask InvokeAsync<T>(string identifier, params object[]? args) => await (await ModuleTask.Value).InvokeAsync<T>(identifier, args);

    public async ValueTask DisposeAsync()
    {
        if (ModuleTask.IsValueCreated)
        {
            var module = await ModuleTask.Value;
            await module.DisposeAsync();
        }
    }
}
