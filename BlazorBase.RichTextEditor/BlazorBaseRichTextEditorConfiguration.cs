using BlazorBase.CRUD.Models;
using BlazorBase.RichTextEditor.Components;
using BlazorBase.RichTextEditor.Models;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlazorBase.RichTextEditor;

public static class BlazorBaseRichTextEditorConfiguration
{
    /// <summary>
    /// Register blazor rich text editor and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBaseRichTextEditor<TOptions>(this IServiceCollection serviceCollection, Action<TOptions>? configureOptions = null)
        where TOptions : class, IBlazorBaseRichTextEditorOptions
    {
        // If options handler is not defined we will get an exception so
        // we need to initialize and empty action.
        if (configureOptions == null)
            configureOptions = (e) => { };

        serviceCollection.AddSingleton(configureOptions)

        .AddSingleton<IBlazorBaseRichTextEditorOptions, TOptions>()
        .AddTransient<IBasePropertyCardInput, BaseRichTextEditorInput>();

        return serviceCollection;
    }

    /// <summary>
    /// Register blazor rich text editor and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBaseRichTextEditor(this IServiceCollection serviceCollection, Action<IBlazorBaseRichTextEditorOptions>? configureOptions = null)
    {
        return AddBlazorBaseRichTextEditor<BlazorBaseRichTextEditorOptions>(serviceCollection, configureOptions);
    }

}
