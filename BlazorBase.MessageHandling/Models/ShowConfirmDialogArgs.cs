using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using Blazorise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Models
{
    public class ShowConfirmDialogArgs: ShowMessageArgs
    {
        public ShowConfirmDialogArgs() { }
        public ShowConfirmDialogArgs(string title, string message, 
                                    MessageType messageType = MessageType.Information,
                                    Func<ModalClosingEventArgs, ConfirmDialogResult, Task>? onClosing = null,
                                    object? icon = null, 
                                    string? confirmButtonText = null,
                                    Color confirmButtonColor = Color.Primary,
                                    string? abortButtonText = null,
                                    Color abortButtonColor = Color.Secondary,
                                    ModalSize modalSize = ModalSize.Large) : base(title, message, messageType, null, icon, abortButtonText, abortButtonColor, modalSize)
        {
            ConfirmButtonText = confirmButtonText;
            ConfirmButtonColor = confirmButtonColor;
            OnClosing = onClosing;
        }

        public string? ConfirmButtonText { get; set; }
        public Color ConfirmButtonColor { get; set; }

        public new Func<ModalClosingEventArgs, ConfirmDialogResult, Task>? OnClosing { get; set; }

    }
}
