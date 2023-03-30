using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using Blazorise;
using Blazorise.Icons.FontAwesome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Models
{
    public class ShowMessageArgs
    {
        public ShowMessageArgs() { }
        public ShowMessageArgs(string title, string message,
                                MessageType messageType = MessageType.Information,
                                Func<ModalClosingEventArgs, Task>? onClosing = null,
                                object? icon = null,
                                string? closeButtonText = null,
                                Color closeButtonColor = Color.Secondary,
                                ModalSize modalSize = ModalSize.Large)
        {
            Title = title;
            Message = message;
            MessageType = messageType;
            OnClosing = onClosing;
            Icon = icon;
            CloseButtonText = closeButtonText;
            CloseButtonColor = closeButtonColor;
            ModalSize = modalSize;
        }

        public string? Title { get; set; }
        public string? Message { get; set; }
        public MessageType MessageType { get; set; }
        public Func<ModalClosingEventArgs, Task>? OnClosing { get; set; }
        public object? Icon { get; set; }
        public string? IconStyle { get; set; }
        public bool IsHandled { get; set; }
        public string? CloseButtonText { get; set; }
        public Color CloseButtonColor { get; set; }
        public ModalSize ModalSize { get; set; }

        public virtual void SetIconByMessageType()
        {
            switch (MessageType)
            {
                case MessageType.Information:
                    Icon = FontAwesomeIcons.InfoCircle;
                    break;
                case MessageType.Error:
                    Icon = FontAwesomeIcons.ExclamationTriangle;
                    IconStyle = "color: red";
                    break;
                case MessageType.Warning:
                    Icon = FontAwesomeIcons.ExclamationTriangle;
                    IconStyle = "color: yellow";
                    break;
            }
        }
    }
}
