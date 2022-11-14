using System;

namespace BlazorBase.RichTextEditor.Models
{
    public class BlazorBaseRichTextEditorOptions
    {
        #region Members

        protected readonly IServiceProvider ServiceProvider;

        protected readonly Action<BlazorBaseRichTextEditorOptions> ConfigureOptions;

        #endregion

        #region Properties
        public Type ImageFileType { get; set; }
        #endregion


        #region Constructors
        public BlazorBaseRichTextEditorOptions(IServiceProvider serviceProvider, Action<BlazorBaseRichTextEditorOptions> configureOptions)
        {
            ServiceProvider = serviceProvider;
            ConfigureOptions = configureOptions;

            ConfigureOptions?.Invoke(this);
        }
        #endregion
    }
}
