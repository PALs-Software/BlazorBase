using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using Blazorise;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Models
{
    public class ShowLoadingProgressMessageArgs : ShowLoadingMessageArgs
    {
        public ShowLoadingProgressMessageArgs() { }
        public ShowLoadingProgressMessageArgs(string message,
                                              int currentProgress = 0,
                                              string progressText = null,                                              
                                              bool showProgressInText = true,
                                              RenderFragment loadingChildContent = null): base(message, loadingChildContent)
        {
            ProgressText = progressText;
            CurrentProgress = currentProgress;
            ShowProgressInText = showProgressInText;
        }

        public string? ProgressText { get; set; }
        public int CurrentProgress { get; set; }
        public bool ShowProgressInText { get; set; }
    }
}
