using BlazorBase.MessageHandling.Interfaces;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Components
{
    public partial class MessageHandler : LayoutComponentBase, IMessageHandler
    {
        public string MessageTitle { get; set; }
        public string Message { get; set; }
        public MessageType MessageType { get; set; }

        private Modal Modal = default!;

        [Inject]
        private IStringLocalizer<MessageHandler> Localizer { get; set; }
        
        public void ShowMessage(string title, string message, MessageType messageType = MessageType.Information)
        {
            this.MessageTitle = title;
            this.Message = message;
            MessageType = messageType;

            Modal.Show();
        }

        private void HideModal()
        {
            Modal.Hide();
        }
    }

    public enum MessageType { 
        Information,
        Error,
        Warning
    }
}
