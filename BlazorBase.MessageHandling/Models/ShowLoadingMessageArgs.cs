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
    public class ShowLoadingMessageArgs
    {
        public ShowLoadingMessageArgs() { }
        public ShowLoadingMessageArgs(string message, RenderFragment loadingChildContent = null)
        {
            Message = message;
            LoadingChildContent = loadingChildContent;
        }

        public Guid Id { get; set; }
        public string? Message { get; set; }
        public RenderFragment LoadingChildContent { get; set; }
        public bool IsHandled { get; set; }
    }
}
