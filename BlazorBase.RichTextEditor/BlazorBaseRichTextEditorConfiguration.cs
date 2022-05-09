using BlazorBase.CRUD.Models;
using BlazorBase.Files.Models;
using BlazorBase.RichTextEditor.Components;
using BlazorBase.RichTextEditor.Models;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlazorBase.RichTextEditor
{
    public static class BlazorBaseRichTextEditorConfiguration
    {
        /// <summary>
        /// Register blazor rich text editor and configures the default behaviour.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddBlazorBaseRichTextEditor<ImageFile>(this IServiceCollection serviceCollection, Action<BlazorBaseRichTextEditorOptions> configureOptions = null)
            where ImageFile : BaseFile
        {
            // If options handler is not defined we will get an exception so
            // we need to initialize and empty action.
            if (configureOptions == null)
                configureOptions = (e) => { e.ImageFileType = typeof(ImageFile); };

            serviceCollection.AddSingleton(configureOptions)

            .AddSingleton<BlazorBaseRichTextEditorOptions>()
            .AddTransient<IBasePropertyCardInput, BaseRichTextEditorInput>();

            return serviceCollection;
        }
    }
}
